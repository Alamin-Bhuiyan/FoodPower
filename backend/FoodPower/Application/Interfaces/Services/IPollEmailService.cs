using System.Threading;
using System.Threading.Tasks;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Services;

public interface IPollEmailService
{
    /// <summary>
    /// Queues the poll-published email broadcast for all active, confirmed users
    /// with an email address. The SMTP work runs fire-and-forget on a background
    /// task; this method returns as soon as the broadcast has been queued.
    /// The poll must have its options loaded.
    /// </summary>
    /// <returns>The number of recipients the broadcast was queued for.</returns>
    Task<int> QueuePollPublishedEmailsAsync(Poll poll, CancellationToken cancellationToken = default);
}
