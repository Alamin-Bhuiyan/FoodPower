using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Push.PushHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Push.PushControllers;

public class GetVapidPublicKeyController(ILogger<GetVapidPublicKeyController> logger) : PushBaseController(logger)
{
    [Tags(SwaggerTag.Push)]
    [Authorize]
    [HttpGet(PushRoutes.GetVapidPublicKeyTemplate, Name = PushRoutes.GetVapidPublicKeyName)]
    public async Task<IActionResult> GetVapidPublicKey(CancellationToken cancellationToken)
    {
        var query = new GetVapidPublicKeyQuery();

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            key => Ok(ToSuccess(key)),
            Problem);

        return response;
    }
}
