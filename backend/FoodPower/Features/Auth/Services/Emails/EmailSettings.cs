namespace FoodPower.Features.Auth.Services.Emails;

public class EmailSettings
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string SenderName { get; set; } = default!;
    public string SenderAddress { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool UseSsl { get; set; }
}
