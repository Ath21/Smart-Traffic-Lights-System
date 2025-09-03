using System;

namespace IntersectionControllerStore.Publishers.LogPub;

public interface ITrafficLogPublisher
{
    Task PublishAuditAsync(string serviceName, string action, string details, object? metadata = null, string? intersectionId = null);
    Task PublishErrorAsync(string serviceName, string errorType, string message, object? metadata = null, string? intersectionId = null);
}
