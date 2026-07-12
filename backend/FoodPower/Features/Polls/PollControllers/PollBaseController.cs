using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Polls.PollControllers;

public class PollBaseController : ApiControllerBase
{
    protected PollBaseController(ILogger<PollBaseController> logger) : base(logger)
    {
    }
}
