using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Settings.SettingsControllers;

public class SettingsBaseController : ApiControllerBase
{
    protected SettingsBaseController(ILogger<SettingsBaseController> logger) : base(logger)
    {
    }
}
