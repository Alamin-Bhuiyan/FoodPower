using FoodPower.Contracts.Requests.Settings;
using FoodPower.Features.Settings.SettingsHandlers;
using Mapster;

namespace FoodPower.Presentation.Mappings;

public class SettingsMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateSettingsRequest, UpdateSettingsCommand>()
            .ConstructUsing(src => new UpdateSettingsCommand(
                src.price_per_lunch,
                src.default_cutoff_time,
                src.time_zone
            ));
    }
}
