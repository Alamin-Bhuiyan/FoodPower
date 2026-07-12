using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Payments;
using FoodPower.Features.Payments.PaymentHandlers;
using FoodPower.Presentation.Routes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Payments.PaymentControllers;

public class CreatePaymentController(ILogger<CreatePaymentController> logger, IAuthUser authUser) : PaymentBaseController(logger)
{
    [Tags(SwaggerTag.Payments)]
    [Authorize]
    [HttpPost(PaymentRoutes.CreatePaymentTemplate, Name = PaymentRoutes.CreatePaymentName)]
    public async Task<IActionResult> CreatePayment(CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreatePaymentCommand>();
        command = command with { UserId = authUser.UserId };

        _ = Task.Run(
            () => _logger.LogInformation(
                "create-payment-request: {Name} by user {UserId} with {AllocationCount} allocation(s)",
                nameof(CreatePaymentCommand),
                authUser.UserId,
                command.Allocations.Count),
            cancellationToken);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            payment => Ok(ToSuccess(payment)),
            Problem);

        return response;
    }
}
