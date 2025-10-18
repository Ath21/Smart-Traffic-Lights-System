using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.User;
using NotificationData.Collections;
using NotificationData.Repositories;
using NotificationData.Repositories.Notifications;
using NotificationStore.Publishers.Logs;
using NotificationStore.Publishers.Notifications;

namespace NotificationStore.Consumers;

public class UserNotificationRequestConsumer : IConsumer<UserNotificationRequest>
{
    private readonly INotificationRepository _repo;
    private readonly INotificationPublisher _publisher;
    private readonly ILogPublisher _logPublisher;

    public UserNotificationRequestConsumer(
        INotificationRepository repo,
        INotificationPublisher publisher,
        ILogPublisher logPublisher)
    {
        _repo = repo;
        _publisher = publisher;
        _logPublisher = logPublisher;
    }

    public async Task Consume(ConsumeContext<UserNotificationRequest> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][USER_REQUEST] Subscription request from {msg.UserId} ({msg.UserEmail}) -> {msg.Intersection}/{msg.Metric}");

        var entity = new NotificationData.Collections.NotificationCollection
        {
            UserId = msg.UserId,
            UserEmail = msg.UserEmail,
            Intersection = msg.Intersection,
            Metric = msg.Metric,
            Type = msg.Type,
            Active = true,
            SubscribedAt = DateTime.UtcNow
        };

        await _repo.AddOrUpdateSubscriptionAsync(entity);

        var verification = new UserNotificationMessage
        {
            UserId = msg.UserId,
            UserEmail = msg.UserEmail,
            Title = "Subscription Verified",
            Body = $"You have successfully subscribed to {msg.Intersection.ToUpper()} [{msg.Metric.ToUpper()}] updates.",
            Type = "private"
        };

        await _publisher.PublishNotificationAsync(verification, "user.notification.{type}");
    }
}
