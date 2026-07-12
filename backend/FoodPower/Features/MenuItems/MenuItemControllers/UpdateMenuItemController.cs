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

public class UpdateMenuItemController(ILogger<UpdateMenuItemController> logger) : MenuItemBaseController(logger)
{
    [Tags(SwaggerTag.MenuItems)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPut(MenuItemRoutes.UpdateMenuItemTemplate, Name = MenuItemRoutes.UpdateMenuItemName)]
    public async Task<IActionResult> UpdateMenuItem(int id, UpdateMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateMenuItemCommand>();
        command = command with { Id = id };

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            menuItem => Ok(ToSuccess(menuItem)),
            Problem);

        return response;
    }
}
