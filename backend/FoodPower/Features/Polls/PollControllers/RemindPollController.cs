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

public class RemindPollController(ILogger<RemindPollController> logger) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(PollRoutes.RemindPollTemplate, Name = PollRoutes.RemindPollName)]
    public async Task<IActionResult> RemindPoll(int id,
        CancellationToken cancellationToken)
    {
        var command = new RemindPollCommand(id);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
