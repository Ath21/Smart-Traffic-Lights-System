using System;

namespace IntersectionControllerStore.Publishers.Logs;

public interface IIntersectionLogPublisher
{
    Task PublishAuditAsync(string action, string message,
        List<int>? lightIds = null, List<string>? lights = null,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null);
    Task PublishErrorAsync(string action, string errorMessage,
        Exception? ex = null, List<int>? lightIds = null, List<string>? lights = null,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null);
    Task PublishFailoverAsync(string action, string message,
        List<int>? lightIds = null, List<string>? lights = null,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null);
}
