using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.Traffic;
using Messages.User;
using NotificationData.Repositories;
using Messages.Traffic.Analytics;
using NotificationData.Repositories.Notifications;
using NotificationData.Repositories.DeliveryLogs;
using NotificationStore.Publishers.Logs;
using NotificationStore.Business.Email;
using NotificationStore.Publishers.Notifications;

namespace NotificationStore.Consumers;

public class IncidentAnalyticsConsumer : IConsumer<IncidentAnalyticsMessage>
{
    private readonly INotificationRepository _repo;
    private readonly DeliveryLogRepository _logRepo;
    private readonly INotificationPublisher _publisher;
    private readonly ILogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    public IncidentAnalyticsConsumer(
        INotificationRepository repo,
        DeliveryLogRepository logRepo,
        INotificationPublisher publisher,
        ILogPublisher logPublisher,
        IEmailService emailService)
    {
        _repo = repo;
        _logRepo = logRepo;
        _publisher = publisher;
        _logPublisher = logPublisher;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<IncidentAnalyticsMessage> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][INCIDENT] Received {msg.IncidentType} ({msg.Severity}) at {msg.Intersection}");

        var subs = await _repo.GetSubscribersAsync(msg.Intersection, "incident");

        foreach (var sub in subs)
        {
            var notif = new UserNotificationMessage
            {
                UserId = sub.UserId,
                UserEmail = sub.UserEmail,
                Title = $"Incident at {msg.Intersection}",
                Body = $"{msg.IncidentType.ToUpper()} ({msg.Severity}) reported at {msg.Timestamp:t}",
                Type = "alert"
            };

            try
            {
                await _publisher.PublishNotificationAsync(notif, "user.notification.{type}");
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.incident", "Success");

                // send email alert
                await _emailService.SendEmailAsync(sub.UserEmail, notif.Title, notif.Body);

                await _logPublisher.PublishAuditAsync(
                    "Consumer",
                    $"[CONSUMER][INCIDENT] Delivered to {sub.UserId} ({sub.UserEmail})");
            }
            catch (Exception ex)
            {
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.incident", "Failed");

                await _logPublisher.PublishErrorAsync(
                    "Consumer",
                    $"[CONSUMER][INCIDENT] Failed delivery for {sub.UserId} ({sub.UserEmail}): {ex.Message}");
            }
        }
    }
}
