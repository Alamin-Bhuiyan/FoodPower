using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Polls.PollHandlers;

public record CastVoteCommand(
    int PollId,
    int PollOptionId,
    int UserId
) : IRequest<ErrorOr<MessageResponse>>;

public class CastVoteCommandValidator : AbstractValidator<CastVoteCommand>
{
    public CastVoteCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");

        RuleFor(x => x.PollOptionId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll_option_id is required.");
    }
}

public class CastVoteCommandHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository)
    : IRequestHandler<CastVoteCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        CastVoteCommand command, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByIdWithOptionsAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        if (poll.Status != PollStatus.Open)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "poll is closed.");
        }

        if (poll.IsCutoffPassed)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "voting is closed. The cutoff time has passed - please contact the admin.");
        }

        if (poll.Options.All(o => o.Id != command.PollOptionId))
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "option does not belong to this poll.");
        }

        var existingVote = await voteRepository.GetByPollAndUserAsync(command.PollId, command.UserId, cancellationToken);

        if (existingVote != null)
        {
            existingVote.PollOptionId = command.PollOptionId;
            existingVote.IsManual = false;
            await voteRepository.UpdateAsync(existingVote, cancellationToken);

            return new MessageResponse("Your vote has been updated.");
        }

        var vote = new Vote(command.PollId, command.PollOptionId, command.UserId, isManual: false, createdById: command.UserId);
        await voteRepository.AddAsync(vote, cancellationToken);

        return new MessageResponse("Your vote has been recorded.");
    }
}
