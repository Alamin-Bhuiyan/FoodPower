using System.Collections.Generic;
using System.Linq;
using FoodPower.Contracts.Responses.Polls;
using FoodPower.Domain.Entities;

namespace FoodPower.Features.Polls.PollHandlers;

public static class PollResponseFactory
{
    public static PollResponse From(Poll poll, List<Vote> votes, int? myVoteOptionId)
    {
        var votesByOption = votes
            .GroupBy(v => v.PollOptionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var options = poll.Options
            .OrderBy(o => o.SortOrder)
            .Select(o =>
            {
                var optionVotes = votesByOption.GetValueOrDefault(o.Id, []);
                return new PollOptionResponse(
                    o.Id,
                    o.MenuItemId,
                    o.Name,
                    o.SortOrder,
                    optionVotes.Count,
                    optionVotes
                        .Select(v => new VoterResponse(
                            v.UserId,
                            v.User?.FullName,
                            v.IsManual,
                            v.CreatedAt))
                        .ToList());
            })
            .ToList();

        return new PollResponse(
            id: poll.Id,
            lunch_date: poll.LunchDate,
            caterer_id: poll.CatererId,
            caterer_name: poll.Caterer?.Name,
            price_per_lunch: poll.PricePerLunch,
            cutoff_at: poll.CutoffAt,
            status: poll.Status.ToString(),
            share_token: poll.ShareToken.ToString(),
            question: poll.Question,
            is_cutoff_passed: poll.IsCutoffPassed,
            total_votes: votes.Count,
            my_vote_option_id: myVoteOptionId,
            options: options,
            created_at: poll.CreatedAt);
    }
}
