using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Features.Polls.PollHandlers;

public record AdminRemoveVoteCommand(
    int PollId,
    int UserId,
    int AdminUserId
) : IRequest<ErrorOr<MessageResponse>>;

public class AdminRemoveVoteCommandValidator : AbstractValidator<AdminRemoveVoteCommand>
{
    public AdminRemoveVoteCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("user id is invalid.");
    }
}

/// <summary>
/// Admin-only vote removal. Unlike the self-service <see cref="RemoveVoteCommand"/>,
/// this deliberately works after the cutoff time and on closed polls, mirroring
/// the flexibility admins already have with manual votes.
/// </summary>
public class AdminRemoveVoteCommandHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository,
    UserManager<AppUser> userManager,
    INotificationService notificationService)
    : IRequestHandler<AdminRemoveVoteCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        AdminRemoveVoteCommand command, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByIdWithOptionsAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        var existingVote = await voteRepository.GetByPollAndUserAsync(command.PollId, command.UserId, cancellationToken);
        if (existingVote == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "this user has not voted on this poll.");
        }

        await voteRepository.DeleteAsync(existingVote, cancellationToken);

        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        var isGeneral = poll.Type == PollType.General;

        await notificationService.CreateForUserAsync(
            userId: command.UserId,
            title: isGeneral ? "Vote removed by admin" : "Lunch removed by admin",
            body: isGeneral
                ? $"An admin removed your vote on \"{poll.Question}\"."
                : $"An admin removed your lunch for {poll.LunchDate:dd MMM yyyy}.",
            type: NotificationType.ManualVoteRemoved,
            refId: poll.Id,
            cancellationToken: cancellationToken);

        return new MessageResponse($"Vote removed for {user?.FullName ?? "user"}.");
    }
}
