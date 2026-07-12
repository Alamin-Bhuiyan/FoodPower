using System;

namespace FoodPower.Contracts.Responses.Notifications;

public record NotificationResponse(
    int id,
    string title,
    string? body,
    string type,
    int? ref_id,
    bool is_read,
    DateTime created_at);
