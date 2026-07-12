namespace FoodPower.Contracts.Requests.Auth;

public class ResetPasswordRequest
{
    public string email { get; set; } = string.Empty;
    public string otp { get; set; } = string.Empty;
    public string new_password { get; set; } = string.Empty;
}
