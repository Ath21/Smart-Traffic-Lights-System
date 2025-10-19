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
    private readonly ILogPublisher _logPublisher;
    private readonly IEmailService _emailService;

    public UserNotificationRequestConsumer(
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

    public async Task Consume(ConsumeContext<UserNotificationRequest> context)
    {
        var msg = context.Message;

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][USER_REQUEST] Subscription from {msg.UserEmail} for {msg.Intersection}/{msg.Metric}");

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

        // -----------------------------------------------------
        // Send verification email to the user
        // -----------------------------------------------------
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
                "Consumer",
                $"[CONSUMER][USER_REQUEST] Verification email sent to {msg.UserEmail}");
        }
        catch (Exception ex)
        {
            await _logRepo.LogDeliveryAsync(msg.UserId, msg.UserEmail, $"{msg.Intersection}.{msg.Metric}.verification", "Failed");

            await _logPublisher.PublishErrorAsync(
                "Consumer",
                $"[CONSUMER][USER_REQUEST] Verification email failed for {msg.UserEmail}: {ex.Message}");
        }

        await _logPublisher.PublishAuditAsync(
            "Consumer",
            $"[CONSUMER][USER_REQUEST] Subscription saved for {msg.UserEmail}");
    }
}
