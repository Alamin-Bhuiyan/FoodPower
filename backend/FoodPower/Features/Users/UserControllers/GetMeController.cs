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

public class GetMeController(ILogger<GetMeController> logger, IAuthUser authUser) : UserBaseController(logger)
{
    [Tags(SwaggerTag.Users)]
    [Authorize]
    [HttpGet(UserRoutes.GetMeTemplate, Name = UserRoutes.GetMeName)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var query = new GetMeQuery(authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            user => Ok(ToSuccess(user)),
            Problem);

        return response;
    }
}
