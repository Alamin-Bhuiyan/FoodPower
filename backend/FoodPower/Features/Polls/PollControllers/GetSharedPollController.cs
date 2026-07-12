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

public class GetSharedPollController(ILogger<GetSharedPollController> logger) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [AllowAnonymous]
    [HttpGet(PollRoutes.GetSharedPollTemplate, Name = PollRoutes.GetSharedPollName)]
    public async Task<IActionResult> GetSharedPoll(string shareToken,
        CancellationToken cancellationToken)
    {
        var query = new GetSharedPollQuery(shareToken);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            poll => Ok(ToSuccess(poll)),
            Problem);

        return response;
    }
}
