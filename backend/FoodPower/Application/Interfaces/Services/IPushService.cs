using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoodPower.Application.Interfaces.Services;

public interface IPushService
{
    /// <summary>
    /// Sends a browser push notification to every stored subscription belonging to
    /// the given users. The subscriptions are loaded on the caller's scope, then the
    /// actual delivery runs fire-and-forget on a background task; this method returns
    /// as soon as the broadcast has been queued. Delivery failures never throw to the
    /// caller, and subscriptions the push service reports as gone (HTTP 404/410) are
    /// removed automatically. No-op when the VAPID keys are not configured.
    /// </summary>
    Task SendToUsersAsync(
        IEnumerable<int> userIds,
        string title,
        string body,
        string? url,
        CancellationToken cancellationToken = default);
}
