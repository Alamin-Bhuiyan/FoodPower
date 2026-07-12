namespace FoodPower.Contracts.Requests.Auth;

public class ChangePasswordRequest
{
    public string old_password { get; set; } = string.Empty;
    public string new_password { get; set; } = string.Empty;
}
