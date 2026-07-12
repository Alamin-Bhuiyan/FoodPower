using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using MediatR;

namespace FoodPower.Features.Caterers.CatererHandlers;

public record GetCaterersQuery() : IRequest<ErrorOr<List<Caterer>>>;

public class GetCaterersQueryHandler(ICatererRepository catererRepository)
    : IRequestHandler<GetCaterersQuery, ErrorOr<List<Caterer>>>
{
    public async Task<ErrorOr<List<Caterer>>> Handle(
        GetCaterersQuery request, CancellationToken cancellationToken)
    {
        var caterers = await catererRepository.ListAsync(cancellationToken);

        return caterers.OrderBy(c => c.Name).ToList();
    }
}
