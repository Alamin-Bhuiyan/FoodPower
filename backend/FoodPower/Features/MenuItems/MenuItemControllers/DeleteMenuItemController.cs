using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.MenuItems.MenuItemHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.MenuItems.MenuItemControllers;

public class DeleteMenuItemController(ILogger<DeleteMenuItemController> logger) : MenuItemBaseController(logger)
{
    [Tags(SwaggerTag.MenuItems)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpDelete(MenuItemRoutes.DeleteMenuItemTemplate, Name = MenuItemRoutes.DeleteMenuItemName)]
    public async Task<IActionResult> DeleteMenuItem(int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMenuItemCommand(id);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
