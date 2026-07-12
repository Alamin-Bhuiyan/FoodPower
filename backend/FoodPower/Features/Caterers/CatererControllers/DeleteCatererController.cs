using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Caterers.CatererHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Caterers.CatererControllers;

public class DeleteCatererController(ILogger<DeleteCatererController> logger) : CatererBaseController(logger)
{
    [Tags(SwaggerTag.Caterers)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpDelete(CatererRoutes.DeleteCatererTemplate, Name = CatererRoutes.DeleteCatererName)]
    public async Task<IActionResult> DeleteCaterer(int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCatererCommand(id);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
