using System;
using System.Globalization;
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

public class GetWeeklySummaryController(ILogger<GetWeeklySummaryController> logger) : DuesBaseController(logger)
{
    [Tags(SwaggerTag.Dues)]
    [Authorize(Roles = PermissionRole.Admin)]
    [HttpGet(DuesRoutes.GetWeeklySummaryTemplate, Name = DuesRoutes.GetWeeklySummaryName)]
    public async Task<IActionResult> GetWeeklySummary(
        [FromQuery] string? weekStart,
        CancellationToken cancellationToken)
    {
        // A supplied value is interpreted as the Monday of the target week.
        // When omitted, the handler defaults to the current week's Monday in the
        // configured timezone (Asia/Dhaka).
        DateTime? start = null;

        if (!string.IsNullOrWhiteSpace(weekStart)
            && DateTime.TryParse(weekStart, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            start = parsed.Date;
        }

        var query = new GetWeeklySummaryQuery(start);

        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);

        var response = result.Match(
            summary => Ok(ToSuccess(summary)),
            Problem);

        return response;
    }
}
