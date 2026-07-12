using System;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Models;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Repositories;

public interface IPollRepository : IEfRepository<Poll>
{
    Task<Poll?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Poll?> GetByIdWithOptionsAsync(int id, CancellationToken cancellationToken = default);
    Task<Poll?> GetByShareTokenAsync(Guid shareToken, CancellationToken cancellationToken = default);
    Task<bool> AnyOpenForDateAsync(DateTime lunchDate, CancellationToken cancellationToken = default);
    Task<PaginatedList<Poll>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
