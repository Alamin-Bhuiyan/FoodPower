using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.MenuItems;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.MenuItems.MenuItemHandlers;

public record UpdateMenuItemCommand(
    int Id,
    string Name,
    string? Description,
    int? DayOfWeek,
    bool IsActive
) : IRequest<ErrorOr<MenuItemResponse>>;

public class UpdateMenuItemCommandValidator : AbstractValidator<UpdateMenuItemCommand>
{
    public UpdateMenuItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("id is invalid.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("name is required.")
            .MaximumLength(200)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("name can be max of 200 characters.");

        RuleFor(x => x.DayOfWeek!.Value)
            .InclusiveBetween(0, 6)
            .When(x => x.DayOfWeek.HasValue)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("day_of_week must be between 0 (Sunday) and 6 (Saturday).");
    }
}

public class UpdateMenuItemCommandHandler(IMenuItemRepository menuItemRepository)
    : IRequestHandler<UpdateMenuItemCommand, ErrorOr<MenuItemResponse>>
{
    public async Task<ErrorOr<MenuItemResponse>> Handle(
        UpdateMenuItemCommand command, CancellationToken cancellationToken)
    {
        var menuItem = await menuItemRepository.GetByIdWithCatererAsync(command.Id, cancellationToken);
        if (menuItem == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "menu item not found.");
        }

        menuItem.Name = command.Name;
        menuItem.Description = command.Description;
        menuItem.IsActive = command.IsActive;

        if (command.DayOfWeek.HasValue)
        {
            menuItem.DayOfWeek = (DayOfWeek)command.DayOfWeek.Value;
        }

        await menuItemRepository.UpdateAsync(menuItem, cancellationToken);

        return MenuItemResponseFactory.From(menuItem);
    }
}
