using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Push;
using FoodPower.Features.Push.PushHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Push.PushControllers;

public class UnsubscribePushController(ILogger<UnsubscribePushController> logger) : PushBaseController(logger)
{
    [Tags(SwaggerTag.Push)]
    [Authorize]
    [HttpPost(PushRoutes.UnsubscribeTemplate, Name = PushRoutes.UnsubscribeName)]
    public async Task<IActionResult> Unsubscribe(UnsubscribePushRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UnsubscribePushCommand(request.endpoint);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
