using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Payments;
using FoodPower.Features.Payments.PaymentHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Payments.PaymentControllers;

public class RejectPaymentController(ILogger<RejectPaymentController> logger, IAuthUser authUser) : PaymentBaseController(logger)
{
    [Tags(SwaggerTag.Payments)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(PaymentRoutes.RejectPaymentTemplate, Name = PaymentRoutes.RejectPaymentName)]
    public async Task<IActionResult> RejectPayment(int id, RejectPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectPaymentCommand(id, request.reason, authUser.UserId);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            payment => Ok(ToSuccess(payment)),
            Problem);

        return response;
    }
}
