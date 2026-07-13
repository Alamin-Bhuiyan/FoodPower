using System.Threading;
using System.Threading.Tasks;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Dues.DuesHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Dues.DuesControllers;

public class GetUserDuesController(ILogger<GetUserDuesController> logger) : DuesBaseController(logger)
{
    [Tags(SwaggerTag.Dues)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpGet(DuesRoutes.GetUserDuesTemplate, Name = DuesRoutes.GetUserDuesName)]
    public async Task<IActionResult> GetUserDues(int userId, CancellationToken cancellationToken)
    {
        var query = new GetUserDuesQuery(userId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            dues => Ok(ToSuccess(dues)),
            Problem);

        return response;
    }
}
