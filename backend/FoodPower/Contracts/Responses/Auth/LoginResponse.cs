using FoodPower.Contracts.Responses.Users;

namespace FoodPower.Contracts.Responses.Auth;

public record LoginResponse(
    string token,
    UserResponse user);
