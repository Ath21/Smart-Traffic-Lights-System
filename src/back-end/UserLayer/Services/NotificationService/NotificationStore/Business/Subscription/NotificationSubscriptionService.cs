using NotificationData.Collections;
using NotificationData.Repositories.Notifications;
using NotificationStore.Models.Requests;
using NotificationStore.Models.Responses;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Business.Subscription;

public class NotificationSubscriptionService : INotificationSubscriptionService
{
    private readonly INotificationRepository _repo;
    private readonly INotificationLogPublisher _logPublisher;

    private const string Domain = "[BUSINESS][SUBSCRIPTION]";

    public NotificationSubscriptionService(
        INotificationRepository repo,
        INotificationLogPublisher logPublisher)
    {
        _repo = repo;
        _logPublisher = logPublisher;
    }

    public async Task<SubscriptionResponse> SubscribeAsync(CreateSubscriptionRequest request)
    {
        var entity = new NotificationCollection
        {
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            Intersection = request.Intersection,
            Metric = request.Metric,
            Active = true,
            SubscribedAt = DateTime.UtcNow
        };

        await _repo.AddOrUpdateSubscriptionAsync(entity);

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} Stored subscription for {request.UserEmail} → {request.Intersection}/{request.Metric}",
            category: "SUBSCRIPTION",
            operation: "SubscribeAsync",
            data: new Dictionary<string, object>
            {
                ["UserId"] = request.UserId,
                ["Email"] = request.UserEmail,
                ["Intersection"] = request.Intersection,
                ["Metric"] = request.Metric
            });

        return new SubscriptionResponse
        {
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            Intersection = request.Intersection,
            Metric = request.Metric,
            Active = true,
            SubscribedAt = entity.SubscribedAt
        };
    }

    public async Task<IEnumerable<SubscriptionResponse>> GetUserSubscriptionsAsync(string userId)
    {
        var subs = await _repo.GetUserSubscriptionsAsync(userId);

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} Retrieved subscriptions for {userId}",
            category: "SUBSCRIPTION",
            operation: "GetUserSubscriptionsAsync",
            data: new Dictionary<string, object> { ["UserId"] = userId, ["Count"] = subs.Count() });

        return subs.Select(s => new SubscriptionResponse
        {
            UserId = s.UserId,
            UserEmail = s.UserEmail,
            Intersection = s.Intersection,
            Metric = s.Metric,
            Active = s.Active,
            SubscribedAt = s.SubscribedAt
        });
    }

    public async Task UnsubscribeAsync(UnsubscribeRequest request)
    {
        if (request.Intersection == null || request.Metric == null)
        {
            var subs = await _repo.GetUserSubscriptionsAsync(request.UserId);
            foreach (var s in subs)
                await _repo.DeactivateSubscriptionAsync(request.UserId, s.UserEmail, s.Intersection, s.Metric);
        }
        else
        {
            await _repo.DeactivateSubscriptionAsync(
                request.UserId, request.UserEmail, request.Intersection, request.Metric);
        }

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} {request.UserEmail} unsubscribed from {request.Intersection ?? "all"}/{request.Metric ?? "all"}",
            category: "SUBSCRIPTION",
            operation: "UnsubscribeAsync",
            data: new Dictionary<string, object>
            {
                ["UserId"] = request.UserId,
                ["Email"] = request.UserEmail,
                ["Intersection"] = request.Intersection ?? "all",
                ["Metric"] = request.Metric ?? "all"
            });
    }
}
