using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Responses.Settings;
using MediatR;

namespace FoodPower.Features.Settings.SettingsHandlers;

public record GetSettingsQuery() : IRequest<ErrorOr<SettingsResponse>>;

public class GetSettingsQueryHandler(ISettingsRepository settingsRepository)
    : IRequestHandler<GetSettingsQuery, ErrorOr<SettingsResponse>>
{
    public async Task<ErrorOr<SettingsResponse>> Handle(
        GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.GetAllAsync(cancellationToken);

        var priceValue = settings.GetValueOrDefault(SettingKeys.PricePerLunch, SettingKeys.DefaultPricePerLunch);
        if (!decimal.TryParse(priceValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            priceValue = SettingKeys.DefaultPricePerLunch;
        }

        return new SettingsResponse(
            price_per_lunch: priceValue,
            default_cutoff_time: settings.GetValueOrDefault(SettingKeys.DefaultCutoffTime, SettingKeys.DefaultCutoffTimeValue),
            time_zone: settings.GetValueOrDefault(SettingKeys.TimeZone, SettingKeys.DefaultTimeZone));
    }
}
