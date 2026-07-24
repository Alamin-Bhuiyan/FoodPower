using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Payments.PaymentHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Payments.PaymentControllers;

public class CancelPaymentController(ILogger<CancelPaymentController> logger, IAuthUser authUser) : PaymentBaseController(logger)
{
    [Tags(SwaggerTag.Payments)]
    [Authorize]
    [HttpDelete(PaymentRoutes.CancelPaymentTemplate, Name = PaymentRoutes.CancelPaymentName)]
    public async Task<IActionResult> CancelPayment(int id,
        CancellationToken cancellationToken)
    {
        var command = new CancelPaymentCommand(id, authUser.UserId);

        _ = Task.Run(
            () => _logger.LogInformation(
                "cancel-payment-request: {Name} payment {PaymentId} by user {UserId}",
                nameof(CancelPaymentCommand),
                id,
                authUser.UserId),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
