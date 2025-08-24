using System;

namespace NotificationStore.Publishers.Logs;

public interface ILogPublisher
{
    Task PublishAuditLogAsync(string action, string details, object? metadata = null);
    Task PublishErrorLogAsync(string errorType, string message, object? metadata = null, Exception? ex = null);
}
