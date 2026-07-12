using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Polls;
using FoodPower.Features.Polls.PollHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Polls.PollControllers;

public class CastVoteController(ILogger<CastVoteController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize]
    [HttpPost(PollRoutes.CastVoteTemplate, Name = PollRoutes.CastVoteName)]
    public async Task<IActionResult> CastVote(int id, CastVoteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CastVoteCommand(id, request.poll_option_id, authUser.UserId);

        _ = Task.Run(
            () => _logger.LogInformation(
                "cast-vote-request: {Name} {@Request}",
                nameof(CastVoteCommand),
                command),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
