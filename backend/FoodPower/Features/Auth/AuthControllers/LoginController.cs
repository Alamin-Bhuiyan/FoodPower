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

public class LoginController(ILogger<LoginController> logger) : AuthBaseController(logger)
{
    [Tags(SwaggerTag.Auth)]
    [HttpPost(AuthRoutes.LoginTemplate, Name = AuthRoutes.LoginName)]
    public async Task<IActionResult> Login(LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<LoginCommand>();

        _ = Task.Run(
            () => _logger.LogInformation(
                "login-request: {Name} {@Request}",
                nameof(LoginCommand),
                command.Email),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            login => Ok(ToSuccess(login)),
            Problem);

        return response;
    }
}
