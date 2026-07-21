using System;
using System.Collections.Generic;

namespace FoodPower.Contracts.Responses.Dues;

public record WeeklyUserSummaryResponse(
    int user_id,
    string? full_name,
    string? profile_picture,
    int lunch_count,
    decimal amount,
    bool paid);

public record WeeklySummaryResponse(
    DateTime week_start,
    DateTime week_end,
    int total_lunches,
    decimal total_amount,
    List<WeeklyUserSummaryResponse> rows);
