using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Polls.PollHandlers;

public record RemoveVoteCommand(
    int PollId,
    int UserId
) : IRequest<ErrorOr<MessageResponse>>;

public class RemoveVoteCommandValidator : AbstractValidator<RemoveVoteCommand>
{
    public RemoveVoteCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");
    }
}

public class RemoveVoteCommandHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository)
    : IRequestHandler<RemoveVoteCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        RemoveVoteCommand command, CancellationToken cancellationToken)
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

        var existingVote = await voteRepository.GetByPollAndUserAsync(command.PollId, command.UserId, cancellationToken);
        if (existingVote == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "you have not voted on this poll.");
        }

        await voteRepository.DeleteAsync(existingVote, cancellationToken);

        return new MessageResponse("Your vote has been removed.");
    }
}
