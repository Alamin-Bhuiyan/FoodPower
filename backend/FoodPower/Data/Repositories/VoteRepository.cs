using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class VoteRepository(ApplicationDbContext dbContext)
    : EfRepository<Vote>(dbContext), IVoteRepository
{
    public async Task<Vote?> GetByPollAndUserAsync(int pollId, int userId, CancellationToken cancellationToken = default)
        => await DbContext.Votes
            .FirstOrDefaultAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);

    public async Task<List<Vote>> GetVotesByPollAsync(int pollId, CancellationToken cancellationToken = default)
        => await DbContext.Votes
            .Include(v => v.User)
            .Include(v => v.Option)
            .Where(v => v.PollId == pollId)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Dictionary<int, int>> GetCountsByOptionAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var counts = await DbContext.Votes
            .Where(v => v.PollId == pollId)
            .GroupBy(v => v.PollOptionId)
            .Select(g => new { OptionId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(c => c.OptionId, c => c.Count);
    }
}
