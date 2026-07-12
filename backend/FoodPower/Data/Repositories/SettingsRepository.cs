using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.BuildingBlocks.Utilities;
using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class SettingsRepository(ApplicationDbContext dbContext) : ISettingsRepository
{
    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        return setting?.Value;
    }

    public async Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Settings
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
    }

    public async Task UpsertAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.Settings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        if (setting == null)
        {
            await dbContext.Settings.AddAsync(new Setting(key, value), cancellationToken);
        }
        else
        {
            setting.Value = value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<decimal> GetPricePerLunchAsync(CancellationToken cancellationToken = default)
    {
        var value = await GetValueAsync(SettingKeys.PricePerLunch, cancellationToken);

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
            ? price
            : decimal.Parse(SettingKeys.DefaultPricePerLunch, CultureInfo.InvariantCulture);
    }

    public async Task<TimeSpan> GetDefaultCutoffTimeAsync(CancellationToken cancellationToken = default)
    {
        var value = await GetValueAsync(SettingKeys.DefaultCutoffTime, cancellationToken);

        return TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var cutoff)
            ? cutoff
            : TimeSpan.Parse(SettingKeys.DefaultCutoffTimeValue, CultureInfo.InvariantCulture);
    }

    public async Task<TimeZoneInfo> GetTimeZoneAsync(CancellationToken cancellationToken = default)
    {
        var value = await GetValueAsync(SettingKeys.TimeZone, cancellationToken);

        return TimeZoneHelper.Resolve(value ?? SettingKeys.DefaultTimeZone);
    }
}
