using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Responses;
using FoodPower.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Features.Polls.PollHandlers;

public record RemindPollCommand(int PollId) : IRequest<ErrorOr<MessageResponse>>;

public class RemindPollCommandValidator : AbstractValidator<RemindPollCommand>
{
    public RemindPollCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");
    }
}

public class RemindPollCommandHandler(
    IPollRepository pollRepository,
    IPushService pushService,
    ApplicationDbContext dbContext)
    : IRequestHandler<RemindPollCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        RemindPollCommand command, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        // Admins publish/manage polls themselves, so they are excluded from reminders.
        var adminRoleId = await dbContext.Roles
            .Where(r => r.Name == PermissionRole.Admin)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var recipientIds = await dbContext.Users
            .Where(u => u.IsActive && u.EmailConfirmed)
            .Where(u => !dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var lunchDateText = poll.LunchDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        await pushService.SendToUsersAsync(
            recipientIds,
            "Lunch poll reminder",
            $"Don't forget to vote for {lunchDateText}",
            $"/poll/{poll.ShareToken}",
            cancellationToken);

        return new MessageResponse($"Reminder sent to {recipientIds.Count} users.");
    }
}
