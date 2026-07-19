namespace FoodPower.Contracts.Requests.Push;

public class SubscribePushRequest
{
    public string endpoint { get; set; } = string.Empty;
    public string p256dh { get; set; } = string.Empty;
    public string auth { get; set; } = string.Empty;
}
