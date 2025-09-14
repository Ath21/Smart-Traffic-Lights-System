using System;

namespace IntersectionControllerStore.Publishers.LogPub;

public interface ITrafficLogPublisher
{
    Task PublishAuditAsync(string serviceName, string action, string details, string? intersectionId = null, object? metadata = null);
    Task PublishErrorAsync(string serviceName, string errorType, string message, string? intersectionId = null, object? metadata = null);
    Task PublishFailoverAsync(string serviceName, string context, string reason, string mode, string? intersectionId = null, object? metadata = null);
}
