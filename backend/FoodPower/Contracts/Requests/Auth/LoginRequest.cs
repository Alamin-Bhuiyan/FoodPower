namespace FoodPower.Contracts.Requests.Auth;

public class LoginRequest
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}
