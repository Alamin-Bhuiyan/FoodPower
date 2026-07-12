using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Models;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class PollRepository(ApplicationDbContext dbContext)
    : EfRepository<Poll>(dbContext), IPollRepository
{
    public async Task<Poll?> GetActiveAsync(CancellationToken cancellationToken = default)
        => await DbContext.Polls
            .Include(p => p.Options.OrderBy(o => o.SortOrder))
            .Include(p => p.Caterer)
            .Where(p => p.Status == PollStatus.Open)
            .OrderByDescending(p => p.LunchDate)
            .ThenByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Poll?> GetByIdWithOptionsAsync(int id, CancellationToken cancellationToken = default)
        => await DbContext.Polls
            .Include(p => p.Options.OrderBy(o => o.SortOrder))
            .Include(p => p.Caterer)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Poll?> GetByShareTokenAsync(Guid shareToken, CancellationToken cancellationToken = default)
        => await DbContext.Polls
            .Include(p => p.Options.OrderBy(o => o.SortOrder))
            .Include(p => p.Caterer)
            .FirstOrDefaultAsync(p => p.ShareToken == shareToken, cancellationToken);

    public async Task<bool> AnyOpenForDateAsync(DateTime lunchDate, CancellationToken cancellationToken = default)
        => await DbContext.Polls
            .AnyAsync(p => p.Status == PollStatus.Open && p.LunchDate == lunchDate.Date, cancellationToken);

    public async Task<PaginatedList<Poll>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Polls
            .Include(p => p.Options.OrderBy(o => o.SortOrder))
            .Include(p => p.Caterer)
            .OrderByDescending(p => p.LunchDate)
            .ThenByDescending(p => p.Id)
            .AsQueryable();

        return await PaginatedListAsync(pageNumber, pageSize, cancellationToken, query);
    }
}
