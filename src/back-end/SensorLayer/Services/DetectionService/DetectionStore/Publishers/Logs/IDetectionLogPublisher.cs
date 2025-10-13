using Messages.Log;

namespace DetectionStore.Publishers.Logs;

public interface IDetectionLogPublisher
{
    Task<LogMessage> PublishAuditAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null);
    Task<LogMessage> PublishErrorAsync(
        string action,
        string errorMessage,
        Exception? ex = null,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null);    
    Task<LogMessage> PublishFailoverAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null);
}
