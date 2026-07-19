using System;

namespace FoodPower.Domain.Entities;

public class PushSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? User { get; set; }

    public PushSubscription()
    {
    }

    public PushSubscription(int userId, string endpoint, string p256dh, string auth)
    {
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        CreatedAt = DateTime.UtcNow;
    }
}
