using System;
using FoodPower.Domain.Enums;

namespace FoodPower.Domain.Entities;

public class OtpToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser? User { get; set; }

    public OtpToken()
    {
    }

    public OtpToken(int userId, string code, OtpPurpose purpose, int lifetimeMinutes = 10)
    {
        UserId = userId;
        Code = code;
        Purpose = purpose;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);
    }

    public bool IsValid => ConsumedAt == null && ExpiresAt > DateTime.UtcNow;
}
