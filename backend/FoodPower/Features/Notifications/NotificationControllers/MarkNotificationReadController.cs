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

public class MarkNotificationReadController(ILogger<MarkNotificationReadController> logger, IAuthUser authUser) : NotificationBaseController(logger)
{
    [Tags(SwaggerTag.Notifications)]
    [Authorize]
    [HttpPost(NotificationRoutes.MarkReadTemplate, Name = NotificationRoutes.MarkReadName)]
    public async Task<IActionResult> MarkRead(int id,
        CancellationToken cancellationToken)
    {
        var command = new MarkNotificationReadCommand(id, authUser.UserId);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            message => Ok(ToSuccess(message)),
            Problem);

        return response;
    }
}
