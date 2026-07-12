using System.Linq;
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

public record ManualVoteCommand(
    int PollId,
    int UserId,
    int PollOptionId,
    int AdminUserId
) : IRequest<ErrorOr<MessageResponse>>;

public class ManualVoteCommandValidator : AbstractValidator<ManualVoteCommand>
{
    public ManualVoteCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("user_id is required.");

        RuleFor(x => x.PollOptionId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll_option_id is required.");
    }
}

public class ManualVoteCommandHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository,
    UserManager<AppUser> userManager,
    INotificationService notificationService)
    : IRequestHandler<ManualVoteCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        ManualVoteCommand command, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByIdWithOptionsAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        var option = poll.Options.FirstOrDefault(o => o.Id == command.PollOptionId);
        if (option == null)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "option does not belong to this poll.");
        }

        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        var existingVote = await voteRepository.GetByPollAndUserAsync(command.PollId, command.UserId, cancellationToken);

        if (existingVote != null)
        {
            existingVote.PollOptionId = command.PollOptionId;
            existingVote.IsManual = true;
            existingVote.CreatedById = command.AdminUserId;
            await voteRepository.UpdateAsync(existingVote, cancellationToken);
        }
        else
        {
            var vote = new Vote(command.PollId, command.PollOptionId, command.UserId, isManual: true, createdById: command.AdminUserId);
            await voteRepository.AddAsync(vote, cancellationToken);
        }

        await notificationService.CreateForUserAsync(
            userId: command.UserId,
            title: "Lunch added by admin",
            body: $"An admin recorded a lunch for you on {poll.LunchDate:dd MMM yyyy} ({option.Name}).",
            type: NotificationType.ManualVoteAdded,
            refId: poll.Id,
            cancellationToken: cancellationToken);

        return new MessageResponse($"Manual vote recorded for {user.FullName}.");
    }
}
