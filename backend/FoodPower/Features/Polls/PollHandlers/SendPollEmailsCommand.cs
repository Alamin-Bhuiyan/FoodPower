using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Polls.PollHandlers;

public record SendPollEmailsCommand(int PollId) : IRequest<ErrorOr<MessageResponse>>;

public class SendPollEmailsCommandValidator : AbstractValidator<SendPollEmailsCommand>
{
    public SendPollEmailsCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");
    }
}

public class SendPollEmailsCommandHandler(
    IPollRepository pollRepository,
    IPollEmailService pollEmailService)
    : IRequestHandler<SendPollEmailsCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        SendPollEmailsCommand command, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByIdWithOptionsAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        // Allowed for open and closed polls alike: the admin may resend at any time.
        var recipientCount = await pollEmailService.QueuePollPublishedEmailsAsync(poll, cancellationToken);

        return new MessageResponse($"Emails are being sent to {recipientCount} users.");
    }
}
