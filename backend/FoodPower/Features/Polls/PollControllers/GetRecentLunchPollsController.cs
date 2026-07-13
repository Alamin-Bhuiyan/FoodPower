using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Polls.PollHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Polls.PollControllers;

public class GetRecentLunchPollsController(ILogger<GetRecentLunchPollsController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize]
    [HttpGet(PollRoutes.GetRecentLunchPollsTemplate, Name = PollRoutes.GetRecentLunchPollsName)]
    public async Task<IActionResult> GetRecentLunchPolls(CancellationToken cancellationToken)
    {
        var query = new GetRecentLunchPollsQuery(authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            polls => Ok(ToSuccess(polls)),
            Problem);

        return response;
    }
}
