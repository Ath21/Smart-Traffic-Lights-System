namespace UserStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    Task PublishNotificationAsync(
        string notificationType,
        string title,
        string body,
        string recipientEmail,
        string status = "Pending",
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
}