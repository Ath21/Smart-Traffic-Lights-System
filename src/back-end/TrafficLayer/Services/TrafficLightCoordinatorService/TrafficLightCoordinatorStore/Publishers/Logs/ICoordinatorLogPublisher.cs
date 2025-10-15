using System;

namespace TrafficLightCoordinatorStore.Publishers.Logs;

public interface ICoordinatorLogPublisher
{
    Task PublishAuditAsync(string action, string message,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null);
    Task PublishErrorAsync(string action, string message,
        Exception? ex = null, Dictionary<string, string>? metadata = null, Guid? correlationId = null);
    Task PublishFailoverAsync(string action, string message,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null);
}

