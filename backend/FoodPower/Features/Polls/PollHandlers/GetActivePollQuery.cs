using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Polls;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Polls.PollHandlers;

public record GetActivePollQuery(int UserId) : IRequest<ErrorOr<PollResponse>>;

public class GetActivePollQueryHandler(
    IPollRepository pollRepository,
    IVoteRepository voteRepository)
    : IRequestHandler<GetActivePollQuery, ErrorOr<PollResponse>>
{
    public async Task<ErrorOr<PollResponse>> Handle(
        GetActivePollQuery request, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetActiveAsync(cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "no active poll found.");
        }

        var votes = await voteRepository.GetVotesByPollAsync(poll.Id, cancellationToken);
        var myVote = votes.FirstOrDefault(v => v.UserId == request.UserId);

        return PollResponseFactory.From(poll, votes, myVote?.PollOptionId);
    }
}
