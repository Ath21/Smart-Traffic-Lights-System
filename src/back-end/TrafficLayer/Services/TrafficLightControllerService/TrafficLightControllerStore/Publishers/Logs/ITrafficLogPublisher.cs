using System;

namespace TrafficLightControllerStore.Publishers.Logs;

public interface ITrafficLogPublisher
{
    Task PublishAuditAsync(string action, string details, object? metadata = null);
    Task PublishErrorAsync(string errorType, string message, object? metadata = null);
}
