using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Repositories;

public interface IPushSubscriptionRepository : IEfRepository<PushSubscription>
{
    Task AddOrUpdateAsync(PushSubscription subscription, CancellationToken cancellationToken = default);
    Task DeleteByEndpointAsync(string endpoint, CancellationToken cancellationToken = default);
    Task<List<PushSubscription>> GetByUserIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
    Task<List<PushSubscription>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
