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

public class GetPollResultsController(ILogger<GetPollResultsController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize]
    [HttpGet(PollRoutes.GetPollResultsTemplate, Name = PollRoutes.GetPollResultsName)]
    public async Task<IActionResult> GetPollResults(int id,
        CancellationToken cancellationToken)
    {
        var query = new GetPollResultsQuery(id, authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            results => Ok(ToSuccess(results)),
            Problem);

        return response;
    }
}
