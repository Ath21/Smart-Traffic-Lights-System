using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.Traffic;
using Messages.User;
using NotificationData.Repositories;
using Messages.Traffic.Analytics;
using NotificationData.Repositories.Notifications;
using NotificationData.Repositories.DeliveryLogs;
using NotificationStore.Publishers.Logs;
using NotificationStore.Publishers.Notifications;

namespace NotificationStore.Consumers;

public class CongestionAnalyticsConsumer : IConsumer<CongestionAnalyticsMessage>
{
    private readonly INotificationRepository _repo;
    private readonly DeliveryLogRepository _logRepo;
    private readonly INotificationPublisher _publisher;
    private readonly ILogPublisher _logPublisher;

    public CongestionAnalyticsConsumer(
        INotificationRepository repo,
        DeliveryLogRepository logRepo,
        INotificationPublisher publisher,
        ILogPublisher logPublisher)
    {
        _repo = repo;
        _logRepo = logRepo;
        _publisher = publisher;
        _logPublisher = logPublisher;
    }

    public async Task Consume(ConsumeContext<CongestionAnalyticsMessage> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][CONGESTION] {msg.Intersection}: {msg.Status} ({msg.CongestionLevel:P1})");

        var subs = await _repo.GetSubscribersAsync(msg.Intersection, "congestion");

        foreach (var sub in subs)
        {
            var notif = new UserNotificationMessage
            {
                UserId = sub.UserId,
                UserEmail = sub.UserEmail,
                Title = $"Traffic Update: {msg.Intersection}",
                Body = $"Current congestion: {msg.CongestionLevel:P1} ({msg.Status})",
                Type = "public"
            };

            try
            {
                await _publisher.PublishNotificationAsync(notif, "user.notification.{type}");
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.congestion", "Success");
            }
            catch (Exception ex)
            {
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.congestion", "Failed");
                await _logPublisher.PublishErrorAsync(
                    "Consumer",
                    $"[CONSUMER][CONGESTION] Delivery failed for {sub.UserId} ({sub.UserEmail}): {ex.Message}");
            }
        }
    }
}