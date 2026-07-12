using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Notifications.NotificationControllers;

public class NotificationBaseController : ApiControllerBase
{
    protected NotificationBaseController(ILogger<NotificationBaseController> logger) : base(logger)
    {
    }
}
