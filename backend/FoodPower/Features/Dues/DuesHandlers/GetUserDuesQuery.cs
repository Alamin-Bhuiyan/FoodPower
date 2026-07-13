using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Dues;
using MediatR;

namespace FoodPower.Features.Dues.DuesHandlers;

public record GetUserDuesQuery(int UserId) : IRequest<ErrorOr<MyDuesResponse>>;

public class GetUserDuesQueryHandler(IDuesRepository duesRepository)
    : IRequestHandler<GetUserDuesQuery, ErrorOr<MyDuesResponse>>
{
    public async Task<ErrorOr<MyDuesResponse>> Handle(
        GetUserDuesQuery request, CancellationToken cancellationToken)
    {
        var dues = await duesRepository.GetMyDuesAsync(request.UserId, cancellationToken);

        return dues;
    }
}
