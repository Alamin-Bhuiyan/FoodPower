using FoodPower.Contracts.Responses.MenuItems;
using FoodPower.Domain.Entities;

namespace FoodPower.Features.MenuItems.MenuItemHandlers;

public static class MenuItemResponseFactory
{
    public static MenuItemResponse From(MenuItem menuItem) =>
        new(
            id: menuItem.Id,
            caterer_id: menuItem.CatererId,
            caterer_name: menuItem.Caterer?.Name,
            day_of_week: (int)menuItem.DayOfWeek,
            day_name: menuItem.DayOfWeek.ToString(),
            name: menuItem.Name,
            description: menuItem.Description,
            is_active: menuItem.IsActive);
}
