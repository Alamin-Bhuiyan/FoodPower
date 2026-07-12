using System;
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

public class GetMenuItemsController(ILogger<GetMenuItemsController> logger) : MenuItemBaseController(logger)
{
    [Tags(SwaggerTag.MenuItems)]
    [Authorize]
    [HttpGet(MenuItemRoutes.GetMenuItemsTemplate, Name = MenuItemRoutes.GetMenuItemsName)]
    public async Task<IActionResult> GetMenuItems(
        [FromQuery] int? catererId,
        [FromQuery] string? day,
        CancellationToken cancellationToken)
    {
        DayOfWeek? dayOfWeek = null;

        if (!string.IsNullOrWhiteSpace(day))
        {
            if (int.TryParse(day, out var dayNumber) && dayNumber is >= 0 and <= 6)
            {
                dayOfWeek = (DayOfWeek)dayNumber;
            }
            else if (Enum.TryParse<DayOfWeek>(day, ignoreCase: true, out var parsedDay))
            {
                dayOfWeek = parsedDay;
            }
        }

        var query = new GetMenuItemsQuery(catererId, dayOfWeek);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            menuItems => Ok(ToSuccess(menuItems)),
            Problem);

        return response;
    }
}
