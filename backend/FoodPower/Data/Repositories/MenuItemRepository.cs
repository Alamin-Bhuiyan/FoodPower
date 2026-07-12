using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class MenuItemRepository(ApplicationDbContext dbContext)
    : EfRepository<MenuItem>(dbContext), IMenuItemRepository
{
    public async Task<List<MenuItem>> GetListAsync(int? catererId, DayOfWeek? day, CancellationToken cancellationToken = default)
    {
        var query = DbContext.MenuItems
            .Include(m => m.Caterer)
            .AsQueryable();

        if (catererId.HasValue)
        {
            query = query.Where(m => m.CatererId == catererId.Value);
        }

        if (day.HasValue)
        {
            query = query.Where(m => m.DayOfWeek == day.Value);
        }

        return await query
            .OrderBy(m => m.DayOfWeek)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItem?> GetByIdWithCatererAsync(int id, CancellationToken cancellationToken = default)
        => await DbContext.MenuItems
            .Include(m => m.Caterer)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<List<MenuItem>> GetByIdsAsync(List<int> ids, CancellationToken cancellationToken = default)
        => await DbContext.MenuItems
            .Where(m => ids.Contains(m.Id))
            .ToListAsync(cancellationToken);
}
