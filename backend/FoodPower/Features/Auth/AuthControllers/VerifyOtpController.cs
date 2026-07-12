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

public class VerifyOtpController(ILogger<VerifyOtpController> logger) : AuthBaseController(logger)
{
    [Tags(SwaggerTag.Auth)]
    [HttpPost(AuthRoutes.VerifyOtpTemplate, Name = AuthRoutes.VerifyOtpName)]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<VerifyOtpCommand>();

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
