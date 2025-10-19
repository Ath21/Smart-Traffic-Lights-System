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
    private readonly INotificationLogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    private const string Domain = "[CONSUMER][ANALYTICS][SUMMARY]";

    public SummaryAnalyticsConsumer(
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

    public async Task Consume(ConsumeContext<SummaryAnalyticsMessage> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} Summary received for {msg.Intersection}",
            category: "ANALYTICS",
            operation: "Consume",
            data: new Dictionary<string, object>
            {
                ["Intersection"] = msg.Intersection
            });

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
                await _logRepo.LogDeliveryAsync(sub.UserId, sub.UserEmail, $"{msg.Intersection}.summary", "Failed");

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
