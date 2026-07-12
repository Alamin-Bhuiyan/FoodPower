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

public class GetAllDuesController(ILogger<GetAllDuesController> logger) : DuesBaseController(logger)
{
    [Tags(SwaggerTag.Dues)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpGet(DuesRoutes.GetAllDuesTemplate, Name = DuesRoutes.GetAllDuesName)]
    public async Task<IActionResult> GetAllDues(CancellationToken cancellationToken)
    {
        var query = new GetAllDuesQuery();

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            dues => Ok(ToSuccess(dues)),
            Problem);

        return response;
    }
}
