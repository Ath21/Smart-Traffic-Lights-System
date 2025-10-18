namespace NotificationStore.Publishers.Logs;

public interface ILogPublisher
{
    Task PublishAsync(string type, string category, string message, string? correlationId = null, Dictionary<string, object>? data = null);
    Task PublishAuditAsync(string category, string message, string? correlationId = null, Dictionary<string, object>? data = null);
    Task PublishErrorAsync(string category, string message, string? correlationId = null, Dictionary<string, object>? data = null);
    Task PublishFailoverAsync(string category, string message, string? correlationId = null, Dictionary<string, object>? data = null);
}