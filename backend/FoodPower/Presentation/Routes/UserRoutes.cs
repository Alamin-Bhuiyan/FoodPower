namespace FoodPower.Presentation.Routes;

public class UserRoutes
{
    public const string GetUsersMethod = "GET";
    public const string GetUsersName = "foodpower.users.list";
    public const string GetUsersTemplate = "/api/users";

    public const string GetMeMethod = "GET";
    public const string GetMeName = "foodpower.users.me";
    public const string GetMeTemplate = "/api/users/me";

    public const string UploadPhotoMethod = "POST";
    public const string UploadPhotoName = "foodpower.users.upload_photo";
    public const string UploadPhotoTemplate = "/api/users/me/photo";

    public const string DeletePhotoMethod = "DELETE";
    public const string DeletePhotoName = "foodpower.users.delete_photo";
    public const string DeletePhotoTemplate = "/api/users/me/photo";
}
