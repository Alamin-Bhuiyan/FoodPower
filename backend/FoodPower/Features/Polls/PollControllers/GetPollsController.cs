using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Requests;
using FoodPower.Features.Polls.PollHandlers;
using FoodPower.Presentation.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Polls.PollControllers;

public class GetPollsController(ILogger<GetPollsController> logger, IAuthUser authUser) : PollBaseController(logger)
{
    [Tags(SwaggerTag.Polls)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpGet(PollRoutes.GetPollsTemplate, Name = PollRoutes.GetPollsName)]
    public async Task<IActionResult> GetPolls([FromQuery] PaginatorRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = new GetPollsQuery(
            PageNumber: pageRequest.page_number,
            PageSize: pageRequest.page_size,
            UserId: authUser.UserId);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            paginatedList => OkWithPagination(paginatedList, ToSuccess(paginatedList.Items)),
            Problem);

        return response;
    }
}
