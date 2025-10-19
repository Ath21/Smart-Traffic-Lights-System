using MassTransit;
using Messages.Traffic.Analytics;
using NotificationData.Repositories.DeliveryLogs;
using NotificationData.Repositories.Notifications;
using NotificationStore.Business.Email;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Consumers;

public class IncidentAnalyticsConsumer : IConsumer<IncidentAnalyticsMessage>
{
    private readonly INotificationRepository _repo;
    private readonly IDeliveryLogRepository _logRepo;
    private readonly INotificationLogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    private const string Domain = "[CONSUMER][ANALYTICS][INCIDENT]";

    public IncidentAnalyticsConsumer(
        INotificationRepository repo,
        IDeliveryLogRepository logRepo,
        INotificationLogPublisher logPublisher,
        IEmailService emailService)
    {
        _repo = repo;
        _logRepo = logRepo;
        _logPublisher = logPublisher;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<IncidentAnalyticsMessage> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} Received {msg.IncidentType} ({msg.Severity}) at {msg.Intersection}",
            category: "ANALYTICS",
            operation: "Consume",
            data: new Dictionary<string, object>
            {
                ["Intersection"] = msg.Intersection,
                ["IncidentType"] = msg.IncidentType,
                ["Severity"] = msg.Severity
            });

        var subs = await _repo.GetSubscribersAsync(msg.Intersection, "incident");

        foreach (var sub in subs)
        {
            var subject = $"Incident at {msg.Intersection}";
            var body = $"{msg.IncidentType.ToUpper()} ({msg.Severity}) reported at {msg.Timestamp:t}";

            try
            {
                await _emailService.SendEmailAsync(sub.UserEmail, subject, body);
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.incident", "Success");

                await _logPublisher.PublishAuditAsync(
                    domain: Domain,
                    messageText: $"{Domain} Email sent to {sub.UserEmail}",
                    category: "EMAIL",
                    operation: "SendEmail",
                    data: new Dictionary<string, object>
                    {
                        ["UserEmail"] = sub.UserEmail,
                        ["Intersection"] = msg.Intersection
                    });
            }
            catch (Exception ex)
            {
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.incident", "Failed");

                await _logPublisher.PublishErrorAsync(
                    domain: Domain,
                    messageText: $"{Domain} Failed to send email to {sub.UserEmail}: {ex.Message}",
                    operation: "SendEmail",
                    data: new Dictionary<string, object>
                    {
                        ["UserEmail"] = sub.UserEmail,
                        ["Intersection"] = msg.Intersection,
                        ["Error"] = ex.Message
                    });
            }
        }
    }
}
