using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Notifications.NotificationHandlers;

public record MarkNotificationReadCommand(int NotificationId, int UserId) : IRequest<ErrorOr<MessageResponse>>;

public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("notification id is invalid.");
    }
}

public class MarkNotificationReadCommandHandler(INotificationRepository notificationRepository)
    : IRequestHandler<MarkNotificationReadCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(command.NotificationId, cancellationToken);
        if (notification == null || notification.UserId != command.UserId)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await notificationRepository.UpdateAsync(notification, cancellationToken);
        }

        return new MessageResponse("Notification marked as read.");
    }
}
