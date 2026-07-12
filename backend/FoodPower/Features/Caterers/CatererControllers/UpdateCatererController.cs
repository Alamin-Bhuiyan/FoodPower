using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Caterers;
using FoodPower.Contracts.Responses.Caterers;
using FoodPower.Features.Caterers.CatererHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Caterers.CatererControllers;

public class UpdateCatererController(ILogger<UpdateCatererController> logger) : CatererBaseController(logger)
{
    [Tags(SwaggerTag.Caterers)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPut(CatererRoutes.UpdateCatererTemplate, Name = CatererRoutes.UpdateCatererName)]
    public async Task<IActionResult> UpdateCaterer(int id, UpdateCatererRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateCatererCommand>();
        command = command with { Id = id };

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            caterer => Ok(ToSuccess(caterer.Adapt<CatererResponse>())),
            Problem);

        return response;
    }
}
