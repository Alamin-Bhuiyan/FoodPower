using System;

namespace FoodPower.Contracts.Responses.Polls;

public record VoterResponse(
    int user_id,
    string? full_name,
    bool is_manual,
    DateTime voted_at);
