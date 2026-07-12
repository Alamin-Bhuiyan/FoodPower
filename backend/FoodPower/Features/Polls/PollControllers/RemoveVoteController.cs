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

public class RemoveVoteController(ILogger<RemoveVoteController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize]
    [HttpDelete(PollRoutes.RemoveVoteTemplate, Name = PollRoutes.RemoveVoteName)]
    public async Task<IActionResult> RemoveVote(int id, CancellationToken cancellationToken)
    {
        var command = new RemoveVoteCommand(id, authUser.UserId);

        _ = Task.Run(
            () => _logger.LogInformation(
                "remove-vote-request: {Name} {@Request}",
                nameof(RemoveVoteCommand),
                command),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
