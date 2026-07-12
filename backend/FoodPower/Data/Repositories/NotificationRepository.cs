using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class NotificationRepository(ApplicationDbContext dbContext)
    : EfRepository<Notification>(dbContext), INotificationRepository
{
    public async Task<List<Notification>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
        => await DbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

    public async Task<int> MarkAllReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var notifications = await DbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }
}
