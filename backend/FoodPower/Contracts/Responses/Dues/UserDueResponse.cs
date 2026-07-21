namespace FoodPower.Contracts.Responses.Dues;

public record UserDueResponse(
    int user_id,
    string? full_name,
    string? email,
    string? profile_picture,
    int lunch_count,
    decimal total_paid,
    decimal balance);
