using System;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests;
using FoodPower.Domain.Enums;
using FoodPower.Features.Payments.PaymentHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Payments.PaymentControllers;

public class GetPaymentsController(ILogger<GetPaymentsController> logger) : PaymentBaseController(logger)
{
    [Tags(SwaggerTag.Payments)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpGet(PaymentRoutes.GetPaymentsTemplate, Name = PaymentRoutes.GetPaymentsName)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] string? status,
        [FromQuery] PaginatorRequest pageRequest,
        CancellationToken cancellationToken)
    {
        PaymentStatus? paymentStatus = null;

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<PaymentStatus>(status, ignoreCase: true, out var parsed))
        {
            paymentStatus = parsed;
        }

        var query = new GetPaymentsQuery(paymentStatus, pageRequest.page_number, pageRequest.page_size);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            paginatedList => OkWithPagination(paginatedList, ToSuccess(paginatedList.Items)),
            Problem);

        return response;
    }
}
