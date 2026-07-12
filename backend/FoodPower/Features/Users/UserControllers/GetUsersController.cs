using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Users.UserHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Users.UserControllers;

public class GetUsersController(ILogger<GetUsersController> logger) : UserBaseController(logger)
{
    [Tags(SwaggerTag.Users)]
    [Authorize]
    [HttpGet(UserRoutes.GetUsersTemplate, Name = UserRoutes.GetUsersName)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery();

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            users => Ok(ToSuccess(users)),
            Problem);

        return response;
    }
}
