using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Payments.PaymentHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Payments.PaymentControllers;

public class RemindDuePaymentsController(ILogger<RemindDuePaymentsController> logger) : PaymentBaseController(logger)
{
    [Tags(SwaggerTag.Payments)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpPost(PaymentRoutes.RemindDueTemplate, Name = PaymentRoutes.RemindDueName)]
    public async Task<IActionResult> RemindDue(CancellationToken cancellationToken)
    {
        var command = new RemindDuePaymentsCommand();

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
