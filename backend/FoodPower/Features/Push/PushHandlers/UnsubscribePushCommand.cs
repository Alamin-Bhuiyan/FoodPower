using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Push.PushHandlers;

public record UnsubscribePushCommand(string Endpoint) : IRequest<ErrorOr<MessageResponse>>;

public class UnsubscribePushCommandValidator : AbstractValidator<UnsubscribePushCommand>
{
    public UnsubscribePushCommandValidator()
    {
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("endpoint is required.");
    }
}

public class UnsubscribePushCommandHandler(IPushSubscriptionRepository pushSubscriptionRepository)
    : IRequestHandler<UnsubscribePushCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        UnsubscribePushCommand command, CancellationToken cancellationToken)
    {
        await pushSubscriptionRepository.DeleteByEndpointAsync(command.Endpoint, cancellationToken);

        return new MessageResponse("Push subscription removed.");
    }
}
