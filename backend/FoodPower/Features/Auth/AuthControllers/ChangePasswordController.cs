using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Auth;
using FoodPower.Features.Auth.AuthHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Auth.AuthControllers;

public class ChangePasswordController(ILogger<ChangePasswordController> logger, IAuthUser authUser) : AuthBaseController(logger)
{
    [Tags(SwaggerTag.Auth)]
    [Authorize]
    [HttpPost(AuthRoutes.ChangePasswordTemplate, Name = AuthRoutes.ChangePasswordName)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<ChangePasswordCommand>();
        command = command with { UserId = authUser.UserId };

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
