using System.Security.Claims;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Auth.Services;

public class AuthUser : IAuthUser
{
    public int UserId { get; }
    public string? Email { get; }
    public bool IsAdmin { get; }

    public AuthUser(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            UserId = 0;
            return;
        }

        var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                    ?? user.FindFirst("sub")
                    ?? user.FindFirst("userId");

        if (claim != null && int.TryParse(claim.Value, out var uid))
        {
            UserId = uid;
        }
        else
        {
            UserId = 0;
        }

        Email = user.FindFirst(ClaimTypes.Email)?.Value;
        IsAdmin = user.IsInRole(PermissionRole.Admin);
    }
}
