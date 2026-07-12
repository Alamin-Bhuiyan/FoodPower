using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Auth;
using FoodPower.Features.Auth.AuthHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Auth.AuthControllers;

public class ResetPasswordController(ILogger<ResetPasswordController> logger) : AuthBaseController(logger)
{
    [Tags(SwaggerTag.Auth)]
    [HttpPost(AuthRoutes.ResetPasswordTemplate, Name = AuthRoutes.ResetPasswordName)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<ResetPasswordCommand>();

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
