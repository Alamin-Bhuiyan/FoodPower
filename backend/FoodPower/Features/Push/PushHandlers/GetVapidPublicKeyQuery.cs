using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Contracts.Responses.Push;
using FoodPower.Features.Auth.Services.Push;
using MediatR;

namespace FoodPower.Features.Push.PushHandlers;

public record GetVapidPublicKeyQuery : IRequest<ErrorOr<VapidKeyResponse>>;

public class GetVapidPublicKeyQueryHandler(PushSettings pushSettings)
    : IRequestHandler<GetVapidPublicKeyQuery, ErrorOr<VapidKeyResponse>>
{
    public Task<ErrorOr<VapidKeyResponse>> Handle(
        GetVapidPublicKeyQuery request, CancellationToken cancellationToken)
    {
        var response = new VapidKeyResponse(pushSettings.PublicKey ?? string.Empty);

        return Task.FromResult<ErrorOr<VapidKeyResponse>>(response);
    }
}
