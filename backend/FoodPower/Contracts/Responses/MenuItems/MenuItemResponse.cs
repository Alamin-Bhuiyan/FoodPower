namespace FoodPower.Contracts.Responses.MenuItems;

public record MenuItemResponse(
    int id,
    int caterer_id,
    string? caterer_name,
    int day_of_week,
    string day_name,
    string name,
    string? description,
    bool is_active);
