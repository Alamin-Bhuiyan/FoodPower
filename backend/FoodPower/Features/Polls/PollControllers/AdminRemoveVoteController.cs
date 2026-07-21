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

public class AdminRemoveVoteController(ILogger<AdminRemoveVoteController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpDelete(PollRoutes.AdminRemoveVoteTemplate, Name = PollRoutes.AdminRemoveVoteName)]
    public async Task<IActionResult> AdminRemoveVote(int id, int userId,
        CancellationToken cancellationToken)
    {
        var command = new AdminRemoveVoteCommand(id, userId, authUser.UserId);

        _ = Task.Run(
            () => _logger.LogInformation(
                "admin-remove-vote-request: {Name} {@Request}",
                nameof(AdminRemoveVoteCommand),
                command),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
