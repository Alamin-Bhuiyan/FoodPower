namespace FoodPower.Presentation.Routes;

public class NotificationRoutes
{
    public const string GetNotificationsMethod = "GET";
    public const string GetNotificationsName = "foodpower.notifications.list";
    public const string GetNotificationsTemplate = "/api/notifications";

    public const string MarkReadMethod = "POST";
    public const string MarkReadName = "foodpower.notifications.mark_read";
    public const string MarkReadTemplate = "/api/notifications/{id}/read";

    public const string MarkAllReadMethod = "POST";
    public const string MarkAllReadName = "foodpower.notifications.mark_all_read";
    public const string MarkAllReadTemplate = "/api/notifications/read-all";
}
