using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Push;
using FoodPower.Features.Push.PushHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Push.PushControllers;

public class SubscribePushController(ILogger<SubscribePushController> logger, IAuthUser authUser) : PushBaseController(logger)
{
    [Tags(SwaggerTag.Push)]
    [Authorize]
    [HttpPost(PushRoutes.SubscribeTemplate, Name = PushRoutes.SubscribeName)]
    public async Task<IActionResult> Subscribe(SubscribePushRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubscribePushCommand(
            request.endpoint,
            request.p256dh,
            request.auth,
            authUser.UserId);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
