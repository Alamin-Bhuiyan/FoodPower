using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Settings.SettingsHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Settings.SettingsControllers;

public class GetSettingsController(ILogger<GetSettingsController> logger) : SettingsBaseController(logger)
{
    [Tags(SwaggerTag.Settings)]
    [Authorize]
    [HttpGet(SettingsRoutes.GetSettingsTemplate, Name = SettingsRoutes.GetSettingsName)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var query = new GetSettingsQuery();

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            settings => Ok(ToSuccess(settings)),
            Problem);

        return response;
    }
}
