using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.MenuItems;
using FoodPower.Features.MenuItems.MenuItemHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.MenuItems.MenuItemControllers;

public class CreateMenuItemsController(ILogger<CreateMenuItemsController> logger) : MenuItemBaseController(logger)
{
    [Tags(SwaggerTag.MenuItems)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(MenuItemRoutes.CreateMenuItemsTemplate, Name = MenuItemRoutes.CreateMenuItemsName)]
    public async Task<IActionResult> CreateMenuItems(CreateMenuItemsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateMenuItemsCommand>();

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            menuItems => Ok(ToSuccess(menuItems)),
            Problem);

        return response;
    }
}
