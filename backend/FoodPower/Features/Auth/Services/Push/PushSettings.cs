namespace FoodPower.Features.Auth.Services.Push;

public class PushSettings
{
    public string PublicKey { get; set; } = default!;
    public string PrivateKey { get; set; } = default!;
    public string Subject { get; set; } = default!;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(PrivateKey);
}
