namespace NotificationStore.Business;

public class NotificationProcessor : INotificationProcessor
{
    private readonly IEmailService _emailSender;
    private readonly ILogger<NotificationProcessor> _logger;

    public NotificationProcessor(IEmailService emailSender, ILogger<NotificationProcessor> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task ProcessNotificationAsync(UserNotificationMessage message)
    {
        switch (message.NotificationType?.ToLower())
        {
            case "traffic":
                await _emailSender.SendEmailAsync(
                    message.RecipientEmail!,
                    $"🚦 Κυκλοφοριακή Ειδοποίηση: {message.Title}",
                    message.Body ?? "Πληροφορίες κυκλοφορίας δεν είναι διαθέσιμες."
                );
                break;

            case "publicnotice":
                await _emailSender.SendEmailAsync(
                    message.RecipientEmail!,
                    $"📢 Ανακοίνωση Πανεπιστημιούπολης: {message.Title}",
                    message.Body ?? string.Empty
                );
                break;

            case "request":
                await _emailSender.SendEmailAsync(
                    message.RecipientEmail!,
                    $"Αίτημα: {message.Title}",
                    message.Body ?? string.Empty
                );
                break;

            default:
                _logger.LogWarning("⚠️ Unknown notification type: {Type}", message.NotificationType);
                break;
        }
    }
}