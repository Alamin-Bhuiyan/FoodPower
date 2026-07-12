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

public class RegisterController(ILogger<RegisterController> logger) : AuthBaseController(logger)
{
    [Tags(SwaggerTag.Auth)]
    [HttpPost(AuthRoutes.RegisterTemplate, Name = AuthRoutes.RegisterName)]
    public async Task<IActionResult> Register(RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RegisterCommand>();

        _ = Task.Run(
            () => _logger.LogInformation(
                "register-request: {Name} {@Request}",
                nameof(RegisterCommand),
                command.Email),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            register => Ok(ToSuccess(register)),
            Problem);

        return response;
    }
}
