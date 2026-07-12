namespace FoodPower.Contracts.Responses.Caterers;

public record CatererResponse(
    int id,
    string? name,
    string? phone,
    decimal price_per_lunch,
    bool is_active);
