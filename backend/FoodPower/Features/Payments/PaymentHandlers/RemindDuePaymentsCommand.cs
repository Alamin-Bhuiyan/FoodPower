using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Responses;
using FoodPower.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Features.Payments.PaymentHandlers;

public record RemindDuePaymentsCommand : IRequest<ErrorOr<MessageResponse>>;

public class RemindDuePaymentsCommandHandler(
    IPushService pushService,
    ApplicationDbContext dbContext)
    : IRequestHandler<RemindDuePaymentsCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        RemindDuePaymentsCommand command, CancellationToken cancellationToken)
    {
        // Admins do not owe weekly lunch dues, so they are excluded from the reminder.
        var adminRoleId = await dbContext.Roles
            .Where(r => r.Name == PermissionRole.Admin)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var recipientIds = await dbContext.Users
            .Where(u => u.IsActive && u.EmailConfirmed)
            .Where(u => !dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        await pushService.SendToUsersAsync(
            recipientIds,
            "Weekly payment reminder",
            "Please complete your weekly lunch payment.",
            "/payments",
            cancellationToken);

        return new MessageResponse($"Reminder sent to {recipientIds.Count} users.");
    }
}
