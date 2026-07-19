using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Push.PushControllers;

public class PushBaseController : ApiControllerBase
{
    protected PushBaseController(ILogger<PushBaseController> logger) : base(logger)
    {
    }
}
