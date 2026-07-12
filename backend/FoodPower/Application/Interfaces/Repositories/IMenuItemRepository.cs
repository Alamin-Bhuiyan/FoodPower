using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Repositories;

public interface IMenuItemRepository : IEfRepository<MenuItem>
{
    Task<List<MenuItem>> GetListAsync(int? catererId, DayOfWeek? day, CancellationToken cancellationToken = default);
    Task<MenuItem?> GetByIdWithCatererAsync(int id, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetByIdsAsync(List<int> ids, CancellationToken cancellationToken = default);
}
