using System;
using FoodPower.Domain.Enums;

namespace FoodPower.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public NotificationType Type { get; set; }
    public int? RefId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? User { get; set; }

    public Notification()
    {
    }

    public Notification(int userId, string title, string? body, NotificationType type, int? refId = null)
    {
        UserId = userId;
        Title = title;
        Body = body;
        Type = type;
        RefId = refId;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }
}
