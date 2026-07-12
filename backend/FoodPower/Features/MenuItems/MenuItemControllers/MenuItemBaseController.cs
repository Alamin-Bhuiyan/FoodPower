using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.MenuItems.MenuItemControllers;

public class MenuItemBaseController : ApiControllerBase
{
    protected MenuItemBaseController(ILogger<MenuItemBaseController> logger) : base(logger)
    {
    }
}
