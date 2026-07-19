namespace FoodPower.Presentation.Routes;

public class PushRoutes
{
    public const string GetVapidPublicKeyMethod = "GET";
    public const string GetVapidPublicKeyName = "foodpower.push.vapid_key";
    public const string GetVapidPublicKeyTemplate = "/api/push/vapid-public-key";

    public const string SubscribeMethod = "POST";
    public const string SubscribeName = "foodpower.push.subscribe";
    public const string SubscribeTemplate = "/api/push/subscribe";

    public const string UnsubscribeMethod = "POST";
    public const string UnsubscribeName = "foodpower.push.unsubscribe";
    public const string UnsubscribeTemplate = "/api/push/unsubscribe";
}
