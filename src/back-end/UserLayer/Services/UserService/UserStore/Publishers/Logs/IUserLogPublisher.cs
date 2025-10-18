namespace UserStore.Publishers.Logs;

public interface IUserLogPublisher
{
    Task PublishAuditAsync(string source, string messageText, string? category = null, Dictionary<string, object>? data = null, string level = "info");
    Task PublishErrorAsync(string source, string messageText, Dictionary<string, object>? data = null);
}