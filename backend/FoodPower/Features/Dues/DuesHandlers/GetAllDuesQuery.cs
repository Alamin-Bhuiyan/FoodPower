using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Dues;
using MediatR;

namespace FoodPower.Features.Dues.DuesHandlers;

public record GetAllDuesQuery() : IRequest<ErrorOr<List<UserDueResponse>>>;

public class GetAllDuesQueryHandler(IDuesRepository duesRepository)
    : IRequestHandler<GetAllDuesQuery, ErrorOr<List<UserDueResponse>>>
{
    public async Task<ErrorOr<List<UserDueResponse>>> Handle(
        GetAllDuesQuery request, CancellationToken cancellationToken)
    {
        var dues = await duesRepository.GetAllDuesAsync(cancellationToken);

        return dues;
    }
}
