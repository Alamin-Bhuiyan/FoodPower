using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.MenuItems;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.MenuItems.MenuItemHandlers;

public record MenuItemInput(string Name, string? Description);

public record CreateMenuItemsCommand(
    int CatererId,
    int DayOfWeek,
    List<MenuItemInput> Items
) : IRequest<ErrorOr<List<MenuItemResponse>>>;

public class CreateMenuItemsCommandValidator : AbstractValidator<CreateMenuItemsCommand>
{
    public CreateMenuItemsCommandValidator()
    {
        RuleFor(x => x.CatererId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("caterer_id is required.");

        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(0, 6)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("day_of_week must be between 0 (Sunday) and 6 (Saturday).");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("at least one menu item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Name)
                .NotEmpty()
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .WithMessage("item name is required.")
                .MaximumLength(200)
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .WithMessage("item name can be max of 200 characters.");
        });
    }
}

public class CreateMenuItemsCommandHandler(
    IMenuItemRepository menuItemRepository,
    ICatererRepository catererRepository)
    : IRequestHandler<CreateMenuItemsCommand, ErrorOr<List<MenuItemResponse>>>
{
    public async Task<ErrorOr<List<MenuItemResponse>>> Handle(
        CreateMenuItemsCommand command, CancellationToken cancellationToken)
    {
        var caterer = await catererRepository.GetByIdAsync(command.CatererId, cancellationToken);
        if (caterer == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "caterer not found.");
        }

        var day = (DayOfWeek)command.DayOfWeek;

        var menuItems = command.Items
            .Select(item => new MenuItem(command.CatererId, day, item.Name, item.Description))
            .ToList();

        await menuItemRepository.AddRangeAsync(menuItems, cancellationToken);

        foreach (var menuItem in menuItems)
        {
            menuItem.Caterer = caterer;
        }

        return menuItems.Select(MenuItemResponseFactory.From).ToList();
    }
}
