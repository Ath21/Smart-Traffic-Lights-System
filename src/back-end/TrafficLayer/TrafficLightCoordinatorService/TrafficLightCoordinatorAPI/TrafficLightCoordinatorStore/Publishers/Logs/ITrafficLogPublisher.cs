namespace TrafficLightCoordinatorStore.Publishers.Logs;

public interface ITrafficLogPublisher
{
    Task PublishAuditAsync(string action, string details, object? metadata, CancellationToken ct);
    Task PublishErrorAsync(string errorType, string message, object? metadata, CancellationToken ct);
}