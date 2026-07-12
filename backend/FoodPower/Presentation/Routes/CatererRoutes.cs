namespace FoodPower.Presentation.Routes;

public class CatererRoutes
{
    public const string GetCaterersMethod = "GET";
    public const string GetCaterersName = "foodpower.caterers.list";
    public const string GetCaterersTemplate = "/api/caterers";

    public const string CreateCatererMethod = "POST";
    public const string CreateCatererName = "foodpower.caterers.create";
    public const string CreateCatererTemplate = "/api/caterers";

    public const string UpdateCatererMethod = "PUT";
    public const string UpdateCatererName = "foodpower.caterers.update";
    public const string UpdateCatererTemplate = "/api/caterers/{id}";

    public const string DeleteCatererMethod = "DELETE";
    public const string DeleteCatererName = "foodpower.caterers.delete";
    public const string DeleteCatererTemplate = "/api/caterers/{id}";
}
