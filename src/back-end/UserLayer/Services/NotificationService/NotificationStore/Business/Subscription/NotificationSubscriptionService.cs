using NotificationData.Collections;
using NotificationData.Repositories.Notifications;
using NotificationStore.Models.Requests;
using NotificationStore.Models.Responses;
using NotificationStore.Publishers.Logs;


namespace NotificationStore.Business.Subscription;

public class NotificationSubscriptionService : INotificationSubscriptionService
{
    private readonly INotificationRepository _repo;
    private readonly ILogPublisher _logPublisher;

    public NotificationSubscriptionService(
        INotificationRepository repo,
        ILogPublisher logPublisher)
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
            Type = request.Type,
            Active = true,
            SubscribedAt = DateTime.UtcNow
        };

        await _repo.AddOrUpdateSubscriptionAsync(entity);

        await _logPublisher.PublishAuditAsync(
            "Business",
            $"[BUSINESS][SUBSCRIPTION] Stored subscription for {request.UserId} ({request.UserEmail}) -> {request.Intersection}/{request.Metric}");

        return new SubscriptionResponse
        {
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            Intersection = request.Intersection,
            Metric = request.Metric,
            Type = request.Type,
            Active = true,
            SubscribedAt = entity.SubscribedAt
        };
    }

    public async Task<IEnumerable<SubscriptionResponse>> GetUserSubscriptionsAsync(string userId)
    {
        var subs = await _repo.GetUserSubscriptionsAsync(userId);

        return subs.Select(s => new SubscriptionResponse
        {
            UserId = s.UserId,
            UserEmail = s.UserEmail,
            Intersection = s.Intersection,
            Metric = s.Metric,
            Type = s.Type,
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
            "Business",
            $"[BUSINESS][SUBSCRIPTION] {request.UserId} ({request.UserEmail}) unsubscribed from {request.Intersection ?? "all"}/{request.Metric ?? "all"}");
    }
}
