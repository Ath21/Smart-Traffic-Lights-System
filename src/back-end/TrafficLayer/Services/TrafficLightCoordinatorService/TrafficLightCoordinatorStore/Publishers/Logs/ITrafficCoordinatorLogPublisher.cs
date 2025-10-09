namespace TrafficLightCoordinatorStore.Publishers.Logs;

public interface ITrafficCoordinatorLogPublisher
{
    Task PublishAuditAsync(string action, string message, Dictionary<string, string>? metadata = null, Guid? correlationId = null);
    Task PublishErrorAsync(string action, string errorMessage, Exception? ex = null, Dictionary<string, string>? metadata = null, Guid? correlationId = null);
    Task PublishFailoverAsync(string action, string message, Dictionary<string, string>? metadata = null, Guid? correlationId = null);
}