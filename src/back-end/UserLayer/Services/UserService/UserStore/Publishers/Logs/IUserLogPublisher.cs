namespace UserStore.Publishers.Logs;

public interface IUserLogPublisher
{
    Task PublishAuditAsync(string action, string message, Dictionary<string, string>? metadata = null);
    Task PublishErrorAsync(string action, string message, Exception? ex = null, Dictionary<string, string>? metadata = null);
    Task PublishFailoverAsync(string action, string message, Dictionary<string, string>? metadata = null);
}