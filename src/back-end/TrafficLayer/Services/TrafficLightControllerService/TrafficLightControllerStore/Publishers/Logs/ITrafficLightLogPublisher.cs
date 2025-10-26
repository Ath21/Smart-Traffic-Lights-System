using System;

namespace TrafficLightControllerStore.Publishers.Logs;

public interface ITrafficLightLogPublisher 
{
    Task PublishAuditAsync(string operation, string message, Dictionary<string, object>? data = null, string? correlationId = null);
    Task PublishErrorAsync(string operation, string message, Exception? ex = null, Dictionary<string, object>? data = null, string? correlationId = null);
    Task PublishFailoverAsync(string operation, string message, Dictionary<string, object>? data = null, string? correlationId = null);
}