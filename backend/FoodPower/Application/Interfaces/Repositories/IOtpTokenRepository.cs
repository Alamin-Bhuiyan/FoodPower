using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;

namespace FoodPower.Application.Interfaces.Repositories;

public interface IOtpTokenRepository : IEfRepository<OtpToken>
{
    Task<OtpToken?> GetLatestValidAsync(int userId, OtpPurpose purpose, CancellationToken cancellationToken = default);
    Task InvalidateAllAsync(int userId, OtpPurpose purpose, CancellationToken cancellationToken = default);
}
