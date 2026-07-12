namespace FoodPower.Presentation.Routes;

public class MenuItemRoutes
{
    public const string GetMenuItemsMethod = "GET";
    public const string GetMenuItemsName = "foodpower.menu_items.list";
    public const string GetMenuItemsTemplate = "/api/menu-items";

    public const string CreateMenuItemsMethod = "POST";
    public const string CreateMenuItemsName = "foodpower.menu_items.create";
    public const string CreateMenuItemsTemplate = "/api/menu-items";

    public const string UpdateMenuItemMethod = "PUT";
    public const string UpdateMenuItemName = "foodpower.menu_items.update";
    public const string UpdateMenuItemTemplate = "/api/menu-items/{id}";

    public const string DeleteMenuItemMethod = "DELETE";
    public const string DeleteMenuItemName = "foodpower.menu_items.delete";
    public const string DeleteMenuItemTemplate = "/api/menu-items/{id}";
}
