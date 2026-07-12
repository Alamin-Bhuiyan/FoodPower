using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Polls;
using FoodPower.Features.Polls.PollHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Polls.PollControllers;

public class CreatePollController(ILogger<CreatePollController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(PollRoutes.CreatePollTemplate, Name = PollRoutes.CreatePollName)]
    public async Task<IActionResult> CreatePoll(CreatePollRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreatePollCommand>();
        command = command with { UserId = authUser.UserId };

        _ = Task.Run(
            () => _logger.LogInformation(
                "create-poll-request: {Name} {@Request}",
                nameof(CreatePollCommand),
                command),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            poll => Ok(ToSuccess(poll)),
            Problem);

        return response;
    }
}
