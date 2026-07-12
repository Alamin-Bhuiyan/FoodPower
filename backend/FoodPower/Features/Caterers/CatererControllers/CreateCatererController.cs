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

public class CreateCatererController(ILogger<CreateCatererController> logger) : CatererBaseController(logger)
{
    [Tags(SwaggerTag.Caterers)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(CatererRoutes.CreateCatererTemplate, Name = CatererRoutes.CreateCatererName)]
    public async Task<IActionResult> CreateCaterer(CreateCatererRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateCatererCommand>();

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            caterer => Ok(ToSuccess(caterer.Adapt<CatererResponse>())),
            Problem);

        return response;
    }
}
