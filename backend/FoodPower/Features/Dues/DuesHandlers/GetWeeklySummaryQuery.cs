using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.BuildingBlocks.Utilities;
using FoodPower.Contracts.Responses.Dues;
using MediatR;

namespace FoodPower.Features.Dues.DuesHandlers;

public record GetWeeklySummaryQuery(DateTime? WeekStart) : IRequest<ErrorOr<WeeklySummaryResponse>>;

public class GetWeeklySummaryQueryHandler(
    IDuesRepository duesRepository,
    ISettingsRepository settingsRepository)
    : IRequestHandler<GetWeeklySummaryQuery, ErrorOr<WeeklySummaryResponse>>
{
    public async Task<ErrorOr<WeeklySummaryResponse>> Handle(
        GetWeeklySummaryQuery request, CancellationToken cancellationToken)
    {
        DateTime weekStart;

        if (request.WeekStart.HasValue)
        {
            // The incoming date is interpreted as the Monday of the target week.
            weekStart = request.WeekStart.Value.Date;
        }
        else
        {
            // Default to the current week's Monday in the configured timezone (Asia/Dhaka).
            var timeZone = await settingsRepository.GetTimeZoneAsync(cancellationToken);
            var todayLocal = TimeZoneHelper.FromUtc(DateTime.UtcNow, timeZone).Date;
            var daysSinceMonday = ((int)todayLocal.DayOfWeek + 6) % 7;
            weekStart = todayLocal.AddDays(-daysSinceMonday);
        }

        var summary = await duesRepository.GetWeeklySummaryAsync(weekStart, cancellationToken);

        return summary;
    }
}
