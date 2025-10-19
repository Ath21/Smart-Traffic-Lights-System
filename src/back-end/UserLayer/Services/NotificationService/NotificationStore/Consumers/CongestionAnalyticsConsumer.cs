using MassTransit;
using Messages.Traffic.Analytics;
using NotificationData.Repositories.DeliveryLogs;
using NotificationData.Repositories.Notifications;
using NotificationStore.Business.Email;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Consumers;

public class CongestionAnalyticsConsumer : IConsumer<CongestionAnalyticsMessage>
{
    private readonly INotificationRepository _repo;
    private readonly IDeliveryLogRepository _logRepo;
    private readonly INotificationLogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    private const string Domain = "[CONSUMER][ANALYTICS][CONGESTION]";

    public CongestionAnalyticsConsumer(
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

    public async Task Consume(ConsumeContext<CongestionAnalyticsMessage> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} Received update for {msg.Intersection}: {msg.Status} ({msg.CongestionLevel:P1})",
            category: "ANALYTICS",
            operation: "Consume",
            data: new Dictionary<string, object>
            {
                ["Intersection"] = msg.Intersection,
                ["Status"] = msg.Status,
                ["Level"] = msg.CongestionLevel
            });

        var subs = await _repo.GetSubscribersAsync(msg.Intersection, "congestion");

        foreach (var sub in subs)
        {
            var subject = $"Traffic Update: {msg.Intersection}";
            var body = $"Current congestion: {msg.CongestionLevel:P1} ({msg.Status})";

            try
            {
                await _emailService.SendEmailAsync(sub.UserEmail, subject, body);
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.congestion", "Success");

                await _logPublisher.PublishAuditAsync(
                    domain: Domain,
                    messageText: $"{Domain} Email delivered to {sub.UserEmail}",
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
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.congestion", "Failed");

                await _logPublisher.PublishErrorAsync(
                    domain: Domain,
                    messageText: $"{Domain} Email failed for {sub.UserEmail}: {ex.Message}",
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
