using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Repositories;

public interface ICatererRepository : IEfRepository<Caterer>
{
    Task<bool> HasMenuItemsAsync(int catererId, CancellationToken cancellationToken = default);
    Task<bool> HasPollsAsync(int catererId, CancellationToken cancellationToken = default);
}
