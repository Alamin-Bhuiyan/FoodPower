using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Models;
using FoodPower.Contracts.Responses.Polls;
using FoodPower.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Features.Polls.PollHandlers;

public record GetPollsQuery(
    int PageNumber,
    int PageSize,
    int UserId
) : IRequest<ErrorOr<PaginatedList<PollResponse>>>;

public class GetPollsQueryValidator : AbstractValidator<GetPollsQuery>
{
    public GetPollsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageNumber != 0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("page_number is invalid!");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageSize != 0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("page_size is invalid!");
    }
}

public class GetPollsQueryHandler(
    IPollRepository pollRepository,
    ApplicationDbContext dbContext)
    : IRequestHandler<GetPollsQuery, ErrorOr<PaginatedList<PollResponse>>>
{
    public async Task<ErrorOr<PaginatedList<PollResponse>>> Handle(
        GetPollsQuery request, CancellationToken cancellationToken)
    {
        var polls = await pollRepository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        var pollIds = polls.Items.Select(p => p.Id).ToList();

        var votes = await dbContext.Votes
            .Include(v => v.User)
            .Where(v => pollIds.Contains(v.PollId))
            .ToListAsync(cancellationToken);

        var myVoteByPoll = votes
            .Where(v => v.UserId == request.UserId)
            .GroupBy(v => v.PollId)
            .ToDictionary(g => g.Key, g => (int?)g.First().PollOptionId);

        var responses = polls.Items.Select(poll =>
        {
            var pollVotes = votes.Where(v => v.PollId == poll.Id).ToList();

            return PollResponseFactory.From(poll, pollVotes, myVoteByPoll.GetValueOrDefault(poll.Id));
        }).ToList();

        return new PaginatedList<PollResponse>(
            responses, polls.TotalCount, polls.PageNumber, request.PageSize <= 0 ? responses.Count : request.PageSize);
    }
}
