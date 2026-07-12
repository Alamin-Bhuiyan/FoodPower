using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Caterers.CatererHandlers;

public record DeleteCatererCommand(int Id) : IRequest<ErrorOr<MessageResponse>>;

public class DeleteCatererCommandValidator : AbstractValidator<DeleteCatererCommand>
{
    public DeleteCatererCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("id is invalid.");
    }
}

public class DeleteCatererCommandHandler(ICatererRepository catererRepository)
    : IRequestHandler<DeleteCatererCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        DeleteCatererCommand command, CancellationToken cancellationToken)
    {
        var caterer = await catererRepository.GetByIdAsync(command.Id, cancellationToken);
        if (caterer == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "caterer not found.");
        }

        var hasPolls = await catererRepository.HasPollsAsync(command.Id, cancellationToken);
        if (hasPolls)
        {
            // Polls snapshot this caterer - keep it for history, deactivate instead.
            caterer.IsActive = false;
            await catererRepository.UpdateAsync(caterer, cancellationToken);
            return new MessageResponse("Caterer has poll history and was deactivated instead of deleted.");
        }

        await catererRepository.DeleteAsync(caterer, cancellationToken);

        return new MessageResponse("Caterer deleted successfully.");
    }
}
