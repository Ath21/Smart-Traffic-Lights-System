using MassTransit;
using Messages.Traffic.Analytics;
using NotificationData.Repositories.DeliveryLogs;
using NotificationData.Repositories.Notifications;
using NotificationStore.Business.Email;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Consumers;

public class SummaryAnalyticsConsumer : IConsumer<SummaryAnalyticsMessage>
{
    private readonly INotificationRepository _repo;
    private readonly IDeliveryLogRepository _logRepo;
    private readonly ILogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    public SummaryAnalyticsConsumer(
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

    public async Task Consume(ConsumeContext<SummaryAnalyticsMessage> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][SUMMARY] Summary received for {msg.Intersection}");

        var subs = await _repo.GetSubscribersAsync(msg.Intersection, "summary");

        foreach (var sub in subs)
        {
            var subject = $"Daily Summary: {msg.Intersection}";
            var body = $"Vehicles: {msg.VehicleCount}, Pedestrians: {msg.PedestrianCount}, Cyclists: {msg.CyclistCount}, " +
                       $"Incidents: {msg.IncidentsDetected}, Avg Congestion: {msg.AverageCongestion:P1}";

            try
            {
                await _emailService.SendEmailAsync(sub.UserEmail, subject, body);
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.summary", "Success");

                await _logPublisher.PublishAuditAsync(
                    "Consumer",
                    $"[CONSUMER][SUMMARY] Email delivered to {sub.UserEmail}");
            }
            catch (Exception ex)
            {
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.summary", "Failed");
                await _logPublisher.PublishErrorAsync(
                    "Consumer",
                    $"[CONSUMER][SUMMARY] Email failed for {sub.UserEmail}: {ex.Message}");
            }
        }
    }
}
