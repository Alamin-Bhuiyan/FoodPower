using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.MenuItems;
using MediatR;

namespace FoodPower.Features.MenuItems.MenuItemHandlers;

public record GetMenuItemsQuery(
    int? CatererId,
    DayOfWeek? Day
) : IRequest<ErrorOr<List<MenuItemResponse>>>;

public class GetMenuItemsQueryHandler(IMenuItemRepository menuItemRepository)
    : IRequestHandler<GetMenuItemsQuery, ErrorOr<List<MenuItemResponse>>>
{
    public async Task<ErrorOr<List<MenuItemResponse>>> Handle(
        GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        var menuItems = await menuItemRepository.GetListAsync(request.CatererId, request.Day, cancellationToken);

        return menuItems.Select(MenuItemResponseFactory.From).ToList();
    }
}
