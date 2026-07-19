using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class PushSubscriptionRepository(ApplicationDbContext dbContext)
    : EfRepository<PushSubscription>(dbContext), IPushSubscriptionRepository
{
    public async Task AddOrUpdateAsync(PushSubscription subscription, CancellationToken cancellationToken = default)
    {
        var existing = await DbContext.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint, cancellationToken);

        if (existing == null)
        {
            await DbContext.PushSubscriptions.AddAsync(subscription, cancellationToken);
        }
        else
        {
            existing.UserId = subscription.UserId;
            existing.P256dh = subscription.P256dh;
            existing.Auth = subscription.Auth;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByEndpointAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var existing = await DbContext.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == endpoint, cancellationToken);

        if (existing != null)
        {
            DbContext.PushSubscriptions.Remove(existing);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<PushSubscription>> GetByUserIdsAsync(
        IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToList();

        return await DbContext.PushSubscriptions
            .Where(s => ids.Contains(s.UserId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PushSubscription>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await DbContext.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
}
