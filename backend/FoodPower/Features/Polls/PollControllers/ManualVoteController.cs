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

public class ManualVoteController(ILogger<ManualVoteController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(PollRoutes.ManualVoteTemplate, Name = PollRoutes.ManualVoteName)]
    public async Task<IActionResult> ManualVote(int id, ManualVoteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ManualVoteCommand(id, request.user_id, request.poll_option_id, authUser.UserId);

        _ = Task.Run(
            () => _logger.LogInformation(
                "manual-vote-request: {Name} {@Request}",
                nameof(ManualVoteCommand),
                command),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
