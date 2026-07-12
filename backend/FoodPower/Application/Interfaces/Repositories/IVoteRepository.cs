using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Repositories;

public interface IVoteRepository : IEfRepository<Vote>
{
    Task<Vote?> GetByPollAndUserAsync(int pollId, int userId, CancellationToken cancellationToken = default);
    Task<List<Vote>> GetVotesByPollAsync(int pollId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, int>> GetCountsByOptionAsync(int pollId, CancellationToken cancellationToken = default);
}
