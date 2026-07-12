using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Notifications;
using MediatR;

namespace FoodPower.Features.Notifications.NotificationHandlers;

public record GetNotificationsQuery(int UserId) : IRequest<ErrorOr<List<NotificationResponse>>>;

public class GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    : IRequestHandler<GetNotificationsQuery, ErrorOr<List<NotificationResponse>>>
{
    public async Task<ErrorOr<List<NotificationResponse>>> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await notificationRepository.GetByUserAsync(request.UserId, cancellationToken);

        return notifications
            .Select(n => new NotificationResponse(
                n.Id,
                n.Title,
                n.Body,
                n.Type.ToString(),
                n.RefId,
                n.IsRead,
                n.CreatedAt))
            .ToList();
    }
}
