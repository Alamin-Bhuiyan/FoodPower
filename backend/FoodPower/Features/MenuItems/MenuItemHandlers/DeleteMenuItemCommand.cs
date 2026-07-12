using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.MenuItems.MenuItemHandlers;

public record DeleteMenuItemCommand(int Id) : IRequest<ErrorOr<MessageResponse>>;

public class DeleteMenuItemCommandValidator : AbstractValidator<DeleteMenuItemCommand>
{
    public DeleteMenuItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("id is invalid.");
    }
}

public class DeleteMenuItemCommandHandler(IMenuItemRepository menuItemRepository)
    : IRequestHandler<DeleteMenuItemCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        DeleteMenuItemCommand command, CancellationToken cancellationToken)
    {
        var menuItem = await menuItemRepository.GetByIdAsync(command.Id, cancellationToken);
        if (menuItem == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "menu item not found.");
        }

        await menuItemRepository.DeleteAsync(menuItem, cancellationToken);

        return new MessageResponse("Menu item deleted successfully.");
    }
}
