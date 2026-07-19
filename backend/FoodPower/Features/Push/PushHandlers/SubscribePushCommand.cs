using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Push.PushHandlers;

public record SubscribePushCommand(
    string Endpoint,
    string P256dh,
    string Auth,
    int UserId
) : IRequest<ErrorOr<MessageResponse>>;

public class SubscribePushCommandValidator : AbstractValidator<SubscribePushCommand>
{
    public SubscribePushCommandValidator()
    {
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("endpoint is required.");

        RuleFor(x => x.P256dh)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("p256dh is required.");

        RuleFor(x => x.Auth)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("auth is required.");
    }
}

public class SubscribePushCommandHandler(IPushSubscriptionRepository pushSubscriptionRepository)
    : IRequestHandler<SubscribePushCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        SubscribePushCommand command, CancellationToken cancellationToken)
    {
        var subscription = new PushSubscription(
            command.UserId,
            command.Endpoint,
            command.P256dh,
            command.Auth);

        await pushSubscriptionRepository.AddOrUpdateAsync(subscription, cancellationToken);

        return new MessageResponse("Push subscription saved.");
    }
}
