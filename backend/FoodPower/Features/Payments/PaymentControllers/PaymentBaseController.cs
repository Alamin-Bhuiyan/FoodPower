using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Payments.PaymentControllers;

public class PaymentBaseController : ApiControllerBase
{
    protected PaymentBaseController(ILogger<PaymentBaseController> logger) : base(logger)
    {
    }
}
