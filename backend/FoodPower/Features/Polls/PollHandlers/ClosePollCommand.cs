using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Polls.PollHandlers;

public record ClosePollCommand(int PollId) : IRequest<ErrorOr<MessageResponse>>;

public class ClosePollCommandValidator : AbstractValidator<ClosePollCommand>
{
    public ClosePollCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");
    }
}

public class ClosePollCommandHandler(IPollRepository pollRepository)
    : IRequestHandler<ClosePollCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        ClosePollCommand command, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        if (poll.Status == PollStatus.Closed)
        {
            return new MessageResponse("Poll is already closed.");
        }

        poll.Status = PollStatus.Closed;
        await pollRepository.UpdateAsync(poll, cancellationToken);

        return new MessageResponse("Poll closed successfully.");
    }
}
