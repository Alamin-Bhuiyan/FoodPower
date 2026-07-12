using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Repositories;

public interface INotificationRepository : IEfRepository<Notification>
{
    Task<List<Notification>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> MarkAllReadAsync(int userId, CancellationToken cancellationToken = default);
}
