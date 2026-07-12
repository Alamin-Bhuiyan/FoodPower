using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using MediatR;

namespace FoodPower.Features.Notifications.NotificationHandlers;

public record MarkAllNotificationsReadCommand(int UserId) : IRequest<ErrorOr<MessageResponse>>;

public class MarkAllNotificationsReadCommandHandler(INotificationRepository notificationRepository)
    : IRequestHandler<MarkAllNotificationsReadCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        MarkAllNotificationsReadCommand command, CancellationToken cancellationToken)
    {
        var count = await notificationRepository.MarkAllReadAsync(command.UserId, cancellationToken);

        return new MessageResponse($"{count} notification(s) marked as read.");
    }
}
