using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Caterers.CatererHandlers;

public record UpdateCatererCommand(
    int Id,
    string Name,
    string? Phone,
    decimal PricePerLunch,
    bool IsActive
) : IRequest<ErrorOr<Caterer>>;

public class UpdateCatererCommandValidator : AbstractValidator<UpdateCatererCommand>
{
    public UpdateCatererCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("id is invalid.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("name is required.")
            .MaximumLength(150)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("name can be max of 150 characters.");

        RuleFor(x => x.PricePerLunch)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("price_per_lunch must be greater than 0.");
    }
}

public class UpdateCatererCommandHandler(ICatererRepository catererRepository)
    : IRequestHandler<UpdateCatererCommand, ErrorOr<Caterer>>
{
    public async Task<ErrorOr<Caterer>> Handle(
        UpdateCatererCommand command, CancellationToken cancellationToken)
    {
        var caterer = await catererRepository.GetByIdAsync(command.Id, cancellationToken);
        if (caterer == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "caterer not found.");
        }

        caterer.Name = command.Name;
        caterer.Phone = command.Phone;
        caterer.PricePerLunch = command.PricePerLunch;
        caterer.IsActive = command.IsActive;

        await catererRepository.UpdateAsync(caterer, cancellationToken);

        return caterer;
    }
}
