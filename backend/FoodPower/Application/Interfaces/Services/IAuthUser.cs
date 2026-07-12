namespace FoodPower.Application.Interfaces.Services;

public interface IAuthUser
{
    public int UserId { get; }
    public string? Email { get; }
    public bool IsAdmin { get; }
}
