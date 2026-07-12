namespace FoodPower.Contracts.Requests.Auth;

public class VerifyOtpRequest
{
    public string email { get; set; } = string.Empty;
    public string otp { get; set; } = string.Empty;
}
