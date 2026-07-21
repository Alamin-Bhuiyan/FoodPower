using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Polls;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Polls.PollHandlers;

public record GetSharedPollQuery(string ShareToken) : IRequest<ErrorOr<SharedPollResponse>>;

public class GetSharedPollQueryValidator : AbstractValidator<GetSharedPollQuery>
{
    public GetSharedPollQueryValidator()
    {
        RuleFor(x => x.ShareToken)
            .Must(value => Guid.TryParse(value, out _))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("share token is invalid.");
    }
}

public class GetSharedPollQueryHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository)
    : IRequestHandler<GetSharedPollQuery, ErrorOr<SharedPollResponse>>
{
    public async Task<ErrorOr<SharedPollResponse>> Handle(
        GetSharedPollQuery request, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByShareTokenAsync(Guid.Parse(request.ShareToken), cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        var votes = await voteRepository.GetVotesByPollAsync(poll.Id, cancellationToken);
        var votesByOption = votes
            .GroupBy(v => v.PollOptionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var options = poll.Options
            .OrderBy(o => o.SortOrder)
            .Select(o =>
            {
                var optionVotes = votesByOption.GetValueOrDefault(o.Id, []);
                return new SharedPollOptionResponse(
                    o.Id,
                    o.Name,
                    o.SortOrder,
                    optionVotes.Count,
                    optionVotes
                        // Shared poll page is public (deep link) — never expose photos there.
                        .Select(v => new VoterResponse(
                            v.UserId,
                            v.User?.FullName,
                            null,
                            v.IsManual,
                            v.CreatedAt))
                        .ToList());
            })
            .ToList();

        return new SharedPollResponse(
            id: poll.Id,
            lunch_date: poll.LunchDate,
            question: poll.Question,
            caterer_name: poll.Caterer?.Name,
            price_per_lunch: poll.PricePerLunch,
            cutoff_at: poll.CutoffAt,
            poll_type: poll.Type,
            status: poll.Status.ToString(),
            share_token: poll.ShareToken.ToString(),
            is_cutoff_passed: poll.IsCutoffPassed,
            total_votes: votes.Count,
            my_vote_option_id: null,
            options: options);
    }
}
