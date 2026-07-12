using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Notifications.NotificationHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Notifications.NotificationControllers;

public class GetNotificationsController(ILogger<GetNotificationsController> logger, IAuthUser authUser) : NotificationBaseController(logger)
{
    [Tags(SwaggerTag.Notifications)]
    [Authorize]
    [HttpGet(NotificationRoutes.GetNotificationsTemplate, Name = NotificationRoutes.GetNotificationsName)]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        var query = new GetNotificationsQuery(authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            notifications => Ok(ToSuccess(notifications)),
            Problem);

        return response;
    }
}
