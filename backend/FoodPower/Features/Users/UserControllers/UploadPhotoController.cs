using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests.Users;
using FoodPower.Features.Users.UserHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Users.UserControllers;

public class UploadPhotoController(ILogger<UploadPhotoController> logger, IAuthUser authUser) : UserBaseController(logger)
{
    [Tags(SwaggerTag.Users)]
    [Authorize]
    [HttpPost(UserRoutes.UploadPhotoTemplate, Name = UserRoutes.UploadPhotoName)]
    public async Task<IActionResult> UploadPhoto(UploadPhotoRequest request, CancellationToken cancellationToken)
    {
        var command = new UploadPhotoCommand(request.image, authUser.UserId);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            user => Ok(ToSuccess(user)),
            Problem);

        return response;
    }
}
