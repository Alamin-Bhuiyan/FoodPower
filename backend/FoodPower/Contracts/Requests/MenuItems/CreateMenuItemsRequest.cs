using System.Collections.Generic;

namespace FoodPower.Contracts.Requests.MenuItems;

public class MenuItemInputRequest
{
    public string name { get; set; } = string.Empty;
    public string? description { get; set; }
}

public class CreateMenuItemsRequest
{
    public int caterer_id { get; set; }
    public int day_of_week { get; set; }
    public List<MenuItemInputRequest> items { get; set; } = [];
}
