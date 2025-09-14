using System;

namespace TrafficLightControllerStore.Publishers.Logs;

public interface ITrafficLogPublisher
{
    Task PublishAuditAsync(
        string action,
        string details,
        string intersection,
        string light,
        object? metadata = null);
    Task PublishErrorAsync(
        string errorType,
        string message,
        string intersection,
        string light,
        object? metadata = null);
}
