using System;
using System.Collections.Generic;
using FoodPower.Domain.Enums;

namespace FoodPower.Contracts.Responses.Polls;

public record SharedPollOptionResponse(
    int id,
    string name,
    int sort_order,
    int vote_count,
    List<VoterResponse> voters);

public record SharedPollResponse(
    int id,
    DateTime lunch_date,
    string question,
    string? caterer_name,
    decimal price_per_lunch,
    DateTime cutoff_at,
    PollType poll_type,
    string status,
    string share_token,
    bool is_cutoff_passed,
    int total_votes,
    int? my_vote_option_id,
    List<SharedPollOptionResponse> options);
