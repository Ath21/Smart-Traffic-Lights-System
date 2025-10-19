using MassTransit;
using Messages.User;
using NotificationData.Repositories.DeliveryLogs;
using NotificationData.Repositories.Notifications;
using NotificationStore.Business.Email;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Consumers;

public class UserNotificationRequestConsumer : IConsumer<UserNotificationRequest>
{
    private readonly INotificationRepository _repo;
    private readonly IDeliveryLogRepository _logRepo;
    private readonly INotificationLogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    private const string Domain = "[CONSUMER][USER_REQUEST]";

    public UserNotificationRequestConsumer(
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

    public async Task Consume(ConsumeContext<UserNotificationRequest> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} Subscription from {msg.UserEmail} for {msg.Intersection}/{msg.Metric}",
            category: "USER_REQUEST",
            operation: "Consume",
            data: new Dictionary<string, object>
            {
                ["UserId"] = msg.UserId,
                ["Email"] = msg.UserEmail,
                ["Intersection"] = msg.Intersection,
                ["Metric"] = msg.Metric
            });

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

        // Send verification email
        var subject = $"Subscription Verification - {msg.Intersection}";
        var body =
            $"Hello {msg.UserEmail},\n\n" +
            $"Your subscription for updates on *{msg.Intersection}* ({msg.Metric}) has been successfully created.\n\n" +
            "You will now receive notifications when new data or alerts are available for this metric.\n\n" +
            "Thank you for using UNIWA STLS.\n\n" +
            "— Notification Service";

        try
        {
            await _emailService.SendEmailAsync(msg.UserEmail, subject, body);
            await _logRepo.LogDeliveryAsync(msg.UserId, msg.UserEmail, $"{msg.Intersection}.{msg.Metric}.verification", "Success");

            await _logPublisher.PublishAuditAsync(
                domain: Domain,
                messageText: $"{Domain} Verification email sent to {msg.UserEmail}",
                category: "EMAIL",
                operation: "SendEmail",
                data: new Dictionary<string, object>
                {
                    ["UserEmail"] = msg.UserEmail,
                    ["Intersection"] = msg.Intersection,
                    ["Metric"] = msg.Metric
                });
        }
        catch (Exception ex)
        {
            await _logRepo.LogDeliveryAsync(msg.UserId, msg.UserEmail, $"{msg.Intersection}.{msg.Metric}.verification", "Failed");

            await _logPublisher.PublishErrorAsync(
                domain: Domain,
                messageText: $"{Domain} Verification email failed for {msg.UserEmail}: {ex.Message}",
                operation: "SendEmail",
                data: new Dictionary<string, object>
                {
                    ["UserEmail"] = msg.UserEmail,
                    ["Intersection"] = msg.Intersection,
                    ["Metric"] = msg.Metric,
                    ["Error"] = ex.Message
                });
        }

        await _logPublisher.PublishAuditAsync(
            domain: Domain,
            messageText: $"{Domain} Subscription saved for {msg.UserEmail}",
            category: "USER_REQUEST",
            operation: "Consume",
            data: new Dictionary<string, object>
            {
                ["UserEmail"] = msg.UserEmail,
                ["Intersection"] = msg.Intersection,
                ["Metric"] = msg.Metric
            });
    }
}
