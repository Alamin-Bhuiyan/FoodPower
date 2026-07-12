using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Enums;

namespace FoodPower.Application.Interfaces.Services;

public interface INotificationService
{
    Task CreateForUserAsync(
        int userId,
        string title,
        string? body,
        NotificationType type,
        int? refId = null,
        CancellationToken cancellationToken = default);

    Task CreateForAllActiveUsersAsync(
        string title,
        string? body,
        NotificationType type,
        int? refId = null,
        CancellationToken cancellationToken = default);
}
