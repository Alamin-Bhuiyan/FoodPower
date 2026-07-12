using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Data;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Features.Auth.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    ApplicationDbContext dbContext) : INotificationService
{
    public async Task CreateForUserAsync(
        int userId,
        string title,
        string? body,
        NotificationType type,
        int? refId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification(userId, title, body, type, refId);
        await notificationRepository.AddAsync(notification, cancellationToken);
    }

    public async Task CreateForAllActiveUsersAsync(
        string title,
        string? body,
        NotificationType type,
        int? refId = null,
        CancellationToken cancellationToken = default)
    {
        var userIds = await dbContext.Users
            .Where(u => u.IsActive && u.EmailConfirmed)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var notifications = userIds
            .Select(userId => new Notification(userId, title, body, type, refId))
            .ToList();

        await notificationRepository.AddRangeAsync(notifications, cancellationToken);
    }
}
