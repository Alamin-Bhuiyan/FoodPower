using System.Linq;
using FoodPower.Contracts.Requests.MenuItems;
using FoodPower.Features.MenuItems.MenuItemHandlers;
using Mapster;

namespace FoodPower.Presentation.Mappings;

public class MenuItemMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateMenuItemsRequest, CreateMenuItemsCommand>()
            .ConstructUsing(src => new CreateMenuItemsCommand(
                src.caterer_id,
                src.day_of_week,
                src.items.Select(i => new MenuItemInput(i.name, i.description)).ToList()
            ));

        config.NewConfig<UpdateMenuItemRequest, UpdateMenuItemCommand>()
            .ConstructUsing(src => new UpdateMenuItemCommand(
                0,
                src.name,
                src.description,
                src.day_of_week,
                src.is_active
            ));
    }
}
