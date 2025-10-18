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
    private readonly ILogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    public CongestionAnalyticsConsumer(
        INotificationRepository repo,
        IDeliveryLogRepository logRepo,
        ILogPublisher logPublisher,
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
            "Consumer",
            $"[CONSUMER][CONGESTION] {msg.Intersection}: {msg.Status} ({msg.CongestionLevel:P1})");

        var subs = await _repo.GetSubscribersAsync(msg.Intersection, "congestion");

        foreach (var sub in subs)
        {
            var subject = $"Traffic Update: {msg.Intersection}";
            var body = $"Current congestion: {msg.CongestionLevel:P1} ({msg.Status})";

            try
            {
                await _emailService.SendEmailAsync(sub.UserEmail, subject, body);
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.congestion", "Success");
            }
            catch (Exception ex)
            {
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.congestion", "Failed");
                await _logPublisher.PublishErrorAsync(
                    "Consumer",
                    $"[CONSUMER][CONGESTION] Email failed for {sub.UserEmail}: {ex.Message}");
            }
        }
    }
}
