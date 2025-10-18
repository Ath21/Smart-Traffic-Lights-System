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

public class SummaryAnalyticsConsumer : IConsumer<SummaryAnalyticsMessage>
{
    private readonly INotificationRepository _repo;
    private readonly DeliveryLogRepository _logRepo;
    private readonly INotificationPublisher _publisher;
    private readonly ILogPublisher _logPublisher;

    public SummaryAnalyticsConsumer(
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

    public async Task Consume(ConsumeContext<SummaryAnalyticsMessage> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][SUMMARY] Summary received for {msg.Intersection}");

        var subs = await _repo.GetSubscribersAsync(msg.Intersection, "summary");

        foreach (var sub in subs)
        {
            var notif = new UserNotificationMessage
            {
                UserId = sub.UserId,
                UserEmail = sub.UserEmail,
                Title = $"Daily Summary: {msg.Intersection}",
                Body = $"Vehicles: {msg.VehicleCount}, Pedestrians: {msg.PedestrianCount}, Cyclists: {msg.CyclistCount}, Incidents: {msg.IncidentsDetected}, Avg Congestion: {msg.AverageCongestion:P1}",
                Type = "public"
            };

            try
            {
                await _publisher.PublishNotificationAsync(notif, "user.notification.{type}");
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.summary", "Success");

                await _logPublisher.PublishAuditAsync(
                    "Consumer",
                    $"[CONSUMER][SUMMARY] Notification delivered to {sub.UserId} ({sub.UserEmail})");
            }
            catch (Exception ex)
            {
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.summary", "Failed");
                await _logPublisher.PublishErrorAsync(
                    "Consumer",
                    $"[CONSUMER][SUMMARY] Delivery failed for {sub.UserId} ({sub.UserEmail}): {ex.Message}");
            }
        }
    }
}
