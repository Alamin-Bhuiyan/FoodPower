namespace FoodPower.Contracts.Requests.Auth;

public class ResendOtpRequest
{
    public string email { get; set; } = string.Empty;
    public string? purpose { get; set; }
}
