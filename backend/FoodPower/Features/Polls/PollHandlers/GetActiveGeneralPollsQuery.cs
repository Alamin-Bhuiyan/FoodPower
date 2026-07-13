using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Polls;
using MediatR;

namespace FoodPower.Features.Polls.PollHandlers;

public record GetActiveGeneralPollsQuery(int UserId) : IRequest<ErrorOr<List<PollResponse>>>;

public class GetActiveGeneralPollsQueryHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository)
    : IRequestHandler<GetActiveGeneralPollsQuery, ErrorOr<List<PollResponse>>>
{
    public async Task<ErrorOr<List<PollResponse>>> Handle(
        GetActiveGeneralPollsQuery request, CancellationToken cancellationToken)
    {
        var polls = await pollRepository.GetActiveGeneralAsync(cancellationToken);

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
