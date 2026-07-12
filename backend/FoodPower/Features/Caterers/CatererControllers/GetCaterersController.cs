using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Responses.Caterers;
using FoodPower.Features.Caterers.CatererHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Caterers.CatererControllers;

public class GetCaterersController(ILogger<GetCaterersController> logger) : CatererBaseController(logger)
{
    [Tags(SwaggerTag.Caterers)]
    [Authorize]
    [HttpGet(CatererRoutes.GetCaterersTemplate, Name = CatererRoutes.GetCaterersName)]
    public async Task<IActionResult> GetCaterers(CancellationToken cancellationToken)
    {
        var query = new GetCaterersQuery();

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            caterers => Ok(ToSuccess(caterers.Select(c => c.Adapt<CatererResponse>()).ToList())),
            Problem);

        return response;
    }
}
