using System;

namespace IntersectionControlStore.Publishers.LogPub;

public interface ITrafficLogPublisher
{
    Task PublishAuditLogAsync(string serviceName, string message, string intersectionId = null);
    Task PublishErrorLogAsync(string serviceName, string message, Exception exception);
}
