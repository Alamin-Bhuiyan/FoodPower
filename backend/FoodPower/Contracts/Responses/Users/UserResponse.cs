using System;
using System.Collections.Generic;

namespace FoodPower.Contracts.Responses.Users;

public record UserResponse(
    int id,
    string? full_name,
    string? email,
    bool is_active,
    bool email_confirmed,
    List<string> roles,
    DateTime? created_at);
