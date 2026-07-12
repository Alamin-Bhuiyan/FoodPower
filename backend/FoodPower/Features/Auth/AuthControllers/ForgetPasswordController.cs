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

public class ForgetPasswordController(ILogger<ForgetPasswordController> logger) : AuthBaseController(logger)
{
    [Tags(SwaggerTag.Auth)]
    [HttpPost(AuthRoutes.ForgetPasswordTemplate, Name = AuthRoutes.ForgetPasswordName)]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<ForgetPasswordCommand>();

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
