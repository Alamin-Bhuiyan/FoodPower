using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class CatererRepository(ApplicationDbContext dbContext)
    : EfRepository<Caterer>(dbContext), ICatererRepository
{
    public async Task<bool> HasMenuItemsAsync(int catererId, CancellationToken cancellationToken = default)
        => await DbContext.MenuItems.AnyAsync(m => m.CatererId == catererId, cancellationToken);

    public async Task<bool> HasPollsAsync(int catererId, CancellationToken cancellationToken = default)
        => await DbContext.Polls.AnyAsync(p => p.CatererId == catererId, cancellationToken);
}
