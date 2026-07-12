namespace FoodPower.Contracts.Requests.MenuItems;

public class UpdateMenuItemRequest
{
    public string name { get; set; } = string.Empty;
    public string? description { get; set; }
    public int? day_of_week { get; set; }
    public bool is_active { get; set; } = true;
}
