namespace FoodPower.Features.Auth.Services;

public class JwtSettings
{
    public string Token { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
