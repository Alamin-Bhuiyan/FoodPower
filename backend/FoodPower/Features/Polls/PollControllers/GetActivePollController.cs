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

public class GetActivePollController(ILogger<GetActivePollController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize]
    [HttpGet(PollRoutes.GetActivePollTemplate, Name = PollRoutes.GetActivePollName)]
    public async Task<IActionResult> GetActivePoll(CancellationToken cancellationToken)
    {
        var query = new GetActivePollQuery(authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            poll => Ok(ToSuccess(poll)),
            Problem);

        return response;
    }
}
