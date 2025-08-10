using System;

namespace PedestrianDetectionStore.Publishers;

public interface IPedestrianDetectionPublisher
{
    Task PublishPedestrianRequestAsync(Guid intersectionId, int count, DateTime timestamp);
    Task PublishAuditLogAsync(string message);
    Task PublishErrorLogAsync(string message, Exception exception);
}
