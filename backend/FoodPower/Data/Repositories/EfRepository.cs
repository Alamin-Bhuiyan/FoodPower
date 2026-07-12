using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class EfRepository<T>(ApplicationDbContext dbContext) : IEfRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext DbContext = dbContext;

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
        => await DbContext.Set<T>().FindAsync([id], cancellationToken);

    public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
        => await DbContext.Set<T>().ToListAsync(cancellationToken);

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<T>().AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();

        await DbContext.Set<T>().AddRangeAsync(entityList, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return entityList;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<T>().Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        DbContext.Set<T>().UpdateRange(entities.ToList());
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<T>().Remove(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        DbContext.Set<T>().RemoveRange(entities);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await DbContext.Set<T>().CountAsync(cancellationToken);

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => await DbContext.Set<T>().AnyAsync(cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await DbContext.SaveChangesAsync(cancellationToken);

    public virtual async Task<PaginatedList<T>> PaginatedListAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default, IQueryable<T>? source = null)
    {
        var query = source ?? DbContext.Set<T>().AsQueryable();
        var result = await ToPaginatedListAsync(query, pageNumber, pageSize, cancellationToken);

        return result;
    }

    protected static async Task<PaginatedList<TItem>> ToPaginatedListAsync<TItem>(
        IQueryable<TItem> source, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        List<TItem> items;
        int count;

        if (pageNumber > 0 && pageSize > 0)
        {
            count = await source.CountAsync(cancellationToken: cancellationToken);
            items = await source.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken: cancellationToken);
        }
        else
        {
            items = await source.ToListAsync(cancellationToken: cancellationToken);
            pageNumber = 1;
            pageSize = count = items.Count;
        }

        return new PaginatedList<TItem>(items, count, pageNumber, pageSize);
    }
}
