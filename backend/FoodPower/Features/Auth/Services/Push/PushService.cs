using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VapidDetails = WebPush.VapidDetails;
using WebPushClient = WebPush.WebPushClient;
using WebPushException = WebPush.WebPushException;
using WebPushSubscription = WebPush.PushSubscription;

namespace FoodPower.Features.Auth.Services.Push;

public class PushService(
    IPushSubscriptionRepository pushSubscriptionRepository,
    PushSettings pushSettings,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PushService> logger) : IPushService
{
    // Guards the "not configured" warning so it is logged at most once per process,
    // even though this service is scoped (a new instance per request).
    private static int _notConfiguredLogged;

    public async Task SendToUsersAsync(
        IEnumerable<int> userIds,
        string title,
        string body,
        string? url,
        CancellationToken cancellationToken = default)
    {
        if (!pushSettings.IsConfigured)
        {
            if (Interlocked.Exchange(ref _notConfiguredLogged, 1) == 0)
            {
                logger.LogWarning(
                    "Web push is not configured (VAPID keys missing); push notifications are disabled.");
            }

            return;
        }

        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var subscriptions = await pushSubscriptionRepository.GetByUserIdsAsync(ids, cancellationToken);
        if (subscriptions.Count == 0)
        {
            return;
        }

        // Payload the service worker receives in its `push` event.
        var payload = JsonSerializer.Serialize(new
        {
            title,
            body,
            url
        });

        var vapidDetails = new VapidDetails(pushSettings.Subject, pushSettings.PublicKey, pushSettings.PrivateKey);

        // Lunch notifications are time-sensitive. Without "Urgency: high" push
        // services treat messages as normal priority and Android Doze / iOS APNs
        // defer them until the device wakes (users then get them only on app open).
        // High urgency is allowed to wake the device for immediate display.
        // TTL: after 4 hours an undelivered lunch notification is useless — drop it.
        var options = new Dictionary<string, object>
        {
            ["vapidDetails"] = vapidDetails,
            ["TTL"] = 14400,
            ["headers"] = new Dictionary<string, object> { ["Urgency"] = "high" }
        };

        // Snapshot the subscription data so the background task never touches the
        // request-scoped DbContext after the response has completed.
        var targets = subscriptions
            .Select(s => (s.Endpoint, s.P256dh, s.Auth))
            .ToList();

        // Fire-and-forget: delivery must never fail or slow down the request, so the
        // WebPush work runs on a fresh DI scope in the background.
        _ = Task.Run(async () =>
        {
            using var client = new WebPushClient();
            var expiredEndpoints = new List<string>();

            foreach (var (endpoint, p256dh, auth) in targets)
            {
                try
                {
                    var subscription = new WebPushSubscription(endpoint, p256dh, auth);
                    await client.SendNotificationAsync(subscription, payload, options);
                }
                catch (WebPushException ex)
                {
                    // 404/410 means the subscription has expired; drop it so it is not retried.
                    if (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                    {
                        expiredEndpoints.Add(endpoint);
                    }
                    else
                    {
                        logger.LogWarning(ex, "web-push send failed for endpoint {Endpoint}.", endpoint);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "web-push send failed for endpoint {Endpoint}.", endpoint);
                }
            }

            if (expiredEndpoints.Count == 0)
            {
                return;
            }

            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IPushSubscriptionRepository>();

                foreach (var endpoint in expiredEndpoints)
                {
                    await repository.DeleteByEndpointAsync(endpoint, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "failed to prune expired push subscriptions.");
            }
        }, CancellationToken.None);
    }
}
