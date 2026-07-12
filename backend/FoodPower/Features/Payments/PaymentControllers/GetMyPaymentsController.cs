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

public class GetMyPaymentsController(ILogger<GetMyPaymentsController> logger, IAuthUser authUser) : PaymentBaseController(logger)
{
    [Tags(SwaggerTag.Payments)]
    [Authorize]
    [HttpGet(PaymentRoutes.GetMyPaymentsTemplate, Name = PaymentRoutes.GetMyPaymentsName)]
    public async Task<IActionResult> GetMyPayments(CancellationToken cancellationToken)
    {
        var query = new GetMyPaymentsQuery(authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            payments => Ok(ToSuccess(payments)),
            Problem);

        return response;
    }
}
