namespace FoodPower.Contracts.Requests.Caterers;

public class UpdateCatererRequest
{
    public string name { get; set; } = string.Empty;
    public string? phone { get; set; }
    public decimal price_per_lunch { get; set; }
    public bool is_active { get; set; } = true;
}
