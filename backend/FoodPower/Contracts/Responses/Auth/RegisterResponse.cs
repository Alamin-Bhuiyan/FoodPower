namespace FoodPower.Contracts.Responses.Auth;

public record RegisterResponse(
    string full_name,
    string email,
    string message);
