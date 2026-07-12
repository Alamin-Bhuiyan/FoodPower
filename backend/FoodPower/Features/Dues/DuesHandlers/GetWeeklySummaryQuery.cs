using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Dues;
using MediatR;

namespace FoodPower.Features.Dues.DuesHandlers;

public record GetWeeklySummaryQuery(DateTime WeekStart) : IRequest<ErrorOr<WeeklySummaryResponse>>;

public class GetWeeklySummaryQueryHandler(IDuesRepository duesRepository)
    : IRequestHandler<GetWeeklySummaryQuery, ErrorOr<WeeklySummaryResponse>>
{
    public async Task<ErrorOr<WeeklySummaryResponse>> Handle(
        GetWeeklySummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await duesRepository.GetWeeklySummaryAsync(request.WeekStart, cancellationToken);

        return summary;
    }
}
