using MassTransit;
using Messages.User;
using NotificationData.Repositories.Notifications;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Consumers;

public class UserNotificationRequestConsumer : IConsumer<UserNotificationRequest>
{
    private readonly INotificationRepository _repo;
    private readonly ILogPublisher _logPublisher;

    public UserNotificationRequestConsumer(
        INotificationRepository repo,
        ILogPublisher logPublisher)
    {
        _repo = repo;
        _logPublisher = logPublisher;
    }

    public async Task Consume(ConsumeContext<UserNotificationRequest> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][USER_REQUEST] Subscription from {msg.UserEmail} for {msg.Intersection}/{msg.Metric}");

        var entity = new NotificationData.Collections.NotificationCollection
        {
            UserId = msg.UserId,
            UserEmail = msg.UserEmail,
            Intersection = msg.Intersection,
            Metric = msg.Metric,
            Active = true,
            SubscribedAt = DateTime.UtcNow
        };

        await _repo.AddOrUpdateSubscriptionAsync(entity);

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][USER_REQUEST] Subscription saved for {msg.UserEmail}");
    }
}
