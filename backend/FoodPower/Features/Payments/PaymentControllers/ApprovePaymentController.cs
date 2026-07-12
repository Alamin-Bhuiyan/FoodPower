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

public class ApprovePaymentController(ILogger<ApprovePaymentController> logger, IAuthUser authUser) : PaymentBaseController(logger)
{
    [Tags(SwaggerTag.Payments)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(PaymentRoutes.ApprovePaymentTemplate, Name = PaymentRoutes.ApprovePaymentName)]
    public async Task<IActionResult> ApprovePayment(int id,
        CancellationToken cancellationToken)
    {
        var command = new ApprovePaymentCommand(id, authUser.UserId);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            payment => Ok(ToSuccess(payment)),
            Problem);

        return response;
    }
}
