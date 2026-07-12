using System;
using System.Collections.Generic;

namespace FoodPower.Contracts.Responses.Polls;

public record PollOptionResponse(
    int id,
    int? menu_item_id,
    string name,
    int sort_order,
    int vote_count,
    List<VoterResponse> voters);

public record PollResponse(
    int id,
    DateTime lunch_date,
    int? caterer_id,
    string? caterer_name,
    decimal price_per_lunch,
    DateTime cutoff_at,
    string status,
    string share_token,
    string question,
    bool is_cutoff_passed,
    int total_votes,
    int? my_vote_option_id,
    List<PollOptionResponse> options,
    DateTime created_at);
