namespace UserStore.Publishers.Logs;

public interface IUserLogPublisher
{
    // log.user.user_service.audit
    Task PublishAuditAsync(string action, string details, object? metadata = null);
    // log.user.user_service.error
    Task PublishErrorAsync(string errorType, string message, object? metadata = null);
}