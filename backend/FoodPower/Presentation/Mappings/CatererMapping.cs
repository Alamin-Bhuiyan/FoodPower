using FoodPower.Contracts.Requests.Caterers;
using FoodPower.Contracts.Responses.Caterers;
using FoodPower.Domain.Entities;
using FoodPower.Features.Caterers.CatererHandlers;
using Mapster;

namespace FoodPower.Presentation.Mappings;

public class CatererMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCatererRequest, CreateCatererCommand>()
            .ConstructUsing(src => new CreateCatererCommand(
                src.name,
                src.phone,
                src.price_per_lunch
            ));

        config.NewConfig<UpdateCatererRequest, UpdateCatererCommand>()
            .ConstructUsing(src => new UpdateCatererCommand(
                0,
                src.name,
                src.phone,
                src.price_per_lunch,
                src.is_active
            ));

        config.NewConfig<Caterer, CatererResponse>()
            .ConstructUsing(src => new CatererResponse(
                src.Id,
                src.Name,
                src.Phone,
                src.PricePerLunch,
                src.IsActive
            ));
    }
}
