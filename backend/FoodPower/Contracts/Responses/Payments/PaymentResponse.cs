using System;
using System.Collections.Generic;

namespace FoodPower.Contracts.Responses.Payments;

public record PaymentAllocationResponse(
    int id,
    int beneficiary_user_id,
    string? beneficiary_name,
    int days,
    decimal amount);

public record PaymentResponse(
    int id,
    int submitted_by_id,
    string? submitted_by_name,
    decimal total_amount,
    string screenshot_path,
    string? note,
    string status,
    int? reviewed_by_id,
    string? reviewed_by_name,
    DateTime? reviewed_at,
    DateTime created_at,
    List<PaymentAllocationResponse> allocations);
