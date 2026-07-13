using System;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Domain.Entities;

public class AppUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ProfilePicturePath { get; set; }
}
