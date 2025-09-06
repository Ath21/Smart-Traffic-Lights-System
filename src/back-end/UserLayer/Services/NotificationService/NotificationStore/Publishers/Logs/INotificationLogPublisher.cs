namespace NotificationStore.Publishers.Logs;

public interface INotificationLogPublisher
{
    // log.user.notification_service.audit
    Task PublishAuditLogAsync(string action, string details, object? metadata = null);
    // log.user.notification_service.error
    Task PublishErrorLogAsync(string errorType, string message, object? metadata = null, Exception? ex = null);
}
