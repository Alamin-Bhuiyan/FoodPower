using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Dues;
using MediatR;

namespace FoodPower.Features.Dues.DuesHandlers;

public record GetMyDuesQuery(int UserId) : IRequest<ErrorOr<MyDuesResponse>>;

public class GetMyDuesQueryHandler(IDuesRepository duesRepository)
    : IRequestHandler<GetMyDuesQuery, ErrorOr<MyDuesResponse>>
{
    public async Task<ErrorOr<MyDuesResponse>> Handle(
        GetMyDuesQuery request, CancellationToken cancellationToken)
    {
        var dues = await duesRepository.GetMyDuesAsync(request.UserId, cancellationToken);

        return dues;
    }
}
