using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Caterers.CatererControllers;

public class CatererBaseController : ApiControllerBase
{
    protected CatererBaseController(ILogger<CatererBaseController> logger) : base(logger)
    {
    }
}
