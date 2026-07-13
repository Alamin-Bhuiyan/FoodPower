using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Polls;
using MediatR;

namespace FoodPower.Features.Polls.PollHandlers;

public record GetRecentLunchPollsQuery(int UserId) : IRequest<ErrorOr<List<PollResponse>>>;

public class GetRecentLunchPollsQueryHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository)
    : IRequestHandler<GetRecentLunchPollsQuery, ErrorOr<List<PollResponse>>>
{
    private const int RecentLimit = 10;

    public async Task<ErrorOr<List<PollResponse>>> Handle(
        GetRecentLunchPollsQuery request, CancellationToken cancellationToken)
    {
        var polls = await pollRepository.GetRecentLunchAsync(RecentLimit, cancellationToken);

        var responses = new List<PollResponse>();

        foreach (var poll in polls)
        {
            var votes = await voteRepository.GetVotesByPollAsync(poll.Id, cancellationToken);
            var myVote = votes.FirstOrDefault(v => v.UserId == request.UserId);

            responses.Add(PollResponseFactory.From(poll, votes, myVote?.PollOptionId));
        }

        return responses;
    }
}
