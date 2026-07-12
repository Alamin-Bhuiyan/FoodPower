using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Caterers.CatererHandlers;

public record CreateCatererCommand(
    string Name,
    string? Phone,
    decimal PricePerLunch
) : IRequest<ErrorOr<Caterer>>;

public class CreateCatererCommandValidator : AbstractValidator<CreateCatererCommand>
{
    public CreateCatererCommandValidator()
    {
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

public class CreateCatererCommandHandler(ICatererRepository catererRepository)
    : IRequestHandler<CreateCatererCommand, ErrorOr<Caterer>>
{
    public async Task<ErrorOr<Caterer>> Handle(
        CreateCatererCommand command, CancellationToken cancellationToken)
    {
        var caterer = new Caterer(command.Name, command.Phone, command.PricePerLunch);

        var result = await catererRepository.AddAsync(caterer, cancellationToken);

        return result.ToErrorOr();
    }
}
