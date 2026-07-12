using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class OtpTokenRepository(ApplicationDbContext dbContext)
    : EfRepository<OtpToken>(dbContext), IOtpTokenRepository
{
    public async Task<OtpToken?> GetLatestValidAsync(int userId, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await DbContext.OtpTokens
            .Where(t => t.UserId == userId
                        && t.Purpose == purpose
                        && t.ConsumedAt == null
                        && t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InvalidateAllAsync(int userId, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var tokens = await DbContext.OtpTokens
            .Where(t => t.UserId == userId
                        && t.Purpose == purpose
                        && t.ConsumedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.ConsumedAt = now;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
