using UserMessages;

namespace UserStore.Publishers.Logs;

public interface IUserLogPublisher
{
    Task PublishAuditAsync(string action, string details, object? metadata = null);
    Task PublishErrorAsync(string errorType, string message, object? metadata = null);
}