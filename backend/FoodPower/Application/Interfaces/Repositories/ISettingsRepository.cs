using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoodPower.Application.Interfaces.Repositories;

public interface ISettingsRepository
{
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(string key, string value, CancellationToken cancellationToken = default);
    Task<decimal> GetPricePerLunchAsync(CancellationToken cancellationToken = default);
    Task<TimeSpan> GetDefaultCutoffTimeAsync(CancellationToken cancellationToken = default);
    Task<TimeZoneInfo> GetTimeZoneAsync(CancellationToken cancellationToken = default);
}
