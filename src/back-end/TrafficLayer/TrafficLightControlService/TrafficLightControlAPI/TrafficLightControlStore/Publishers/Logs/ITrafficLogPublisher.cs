using System;

namespace TrafficLightControlStore.Publishers.Logs;

public interface ITrafficLogPublisher
{
    Task PublishAuditLogAsync(string serviceName, string message);
    Task PublishErrorLogAsync(string serviceName, string message, Exception exception);
}
