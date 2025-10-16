namespace NotificationStore.Publishers.Logs;

public interface INotificationLogPublisher
{
    Task PublishAuditAsync(string action, string message, Dictionary<string, string>? metadata = null);
    Task PublishErrorAsync(string action, string message, Dictionary<string, string>? metadata = null);
}