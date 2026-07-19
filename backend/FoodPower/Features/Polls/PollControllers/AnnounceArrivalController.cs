using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Polls.PollHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Polls.PollControllers;

public class AnnounceArrivalController(ILogger<AnnounceArrivalController> logger) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(PollRoutes.AnnounceArrivalTemplate, Name = PollRoutes.AnnounceArrivalName)]
    public async Task<IActionResult> AnnounceArrival(int id,
        CancellationToken cancellationToken)
    {
        var command = new AnnounceArrivalCommand(id);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
