using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Settings;
using FoodPower.Features.Settings.SettingsHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Settings.SettingsControllers;

public class UpdateSettingsController(ILogger<UpdateSettingsController> logger) : SettingsBaseController(logger)
{
    [Tags(SwaggerTag.Settings)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPut(SettingsRoutes.UpdateSettingsTemplate, Name = SettingsRoutes.UpdateSettingsName)]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateSettingsCommand>();

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            settings => Ok(ToSuccess(settings)),
            Problem);

        return response;
    }
}
