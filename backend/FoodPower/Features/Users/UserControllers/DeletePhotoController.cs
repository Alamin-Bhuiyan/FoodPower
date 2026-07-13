using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Users.UserHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Users.UserControllers;

public class DeletePhotoController(ILogger<DeletePhotoController> logger, IAuthUser authUser) : UserBaseController(logger)
{
    [Tags(SwaggerTag.Users)]
    [Authorize]
    [HttpDelete(UserRoutes.DeletePhotoTemplate, Name = UserRoutes.DeletePhotoName)]
    public async Task<IActionResult> DeletePhoto(CancellationToken cancellationToken)
    {
        var command = new DeletePhotoCommand(authUser.UserId);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            user => Ok(ToSuccess(user)),
            Problem);

        return response;
    }
}
