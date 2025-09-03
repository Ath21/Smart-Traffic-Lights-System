using System;

namespace TrafficLightControlStore.Publishers.Logs;

public interface ITrafficLogPublisher
{
    Task PublishAuditLogAsync(string serviceName, string action, string details, object? metadata = null);
    Task PublishErrorLogAsync(string serviceName, string errorType, string message, object? metadata = null);
}
