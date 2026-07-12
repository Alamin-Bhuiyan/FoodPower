using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Dues.DuesControllers;

public class DuesBaseController : ApiControllerBase
{
    protected DuesBaseController(ILogger<DuesBaseController> logger) : base(logger)
    {
    }
}
