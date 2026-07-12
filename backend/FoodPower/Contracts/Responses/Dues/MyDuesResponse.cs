using System;
using System.Collections.Generic;

namespace FoodPower.Contracts.Responses.Dues;

public record DueHistoryItemResponse(
    DateTime date,
    string type,
    string description,
    decimal amount);

public record MyDuesResponse(
    decimal balance,
    int lunch_count,
    decimal total_paid,
    decimal total_consumed,
    List<DueHistoryItemResponse> history);
