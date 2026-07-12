using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Features.Dues.DuesHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Dues.DuesControllers;

public class GetMyDuesController(ILogger<GetMyDuesController> logger, IAuthUser authUser) : DuesBaseController(logger)
{
    [Tags(SwaggerTag.Dues)]
    [Authorize]
    [HttpGet(DuesRoutes.GetMyDuesTemplate, Name = DuesRoutes.GetMyDuesName)]
    public async Task<IActionResult> GetMyDues(CancellationToken cancellationToken)
    {
        var query = new GetMyDuesQuery(authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            dues => Ok(ToSuccess(dues)),
            Problem);

        return response;
    }
}
