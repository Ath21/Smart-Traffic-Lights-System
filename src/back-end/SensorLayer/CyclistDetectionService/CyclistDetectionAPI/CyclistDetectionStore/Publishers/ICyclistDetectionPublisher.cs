using System;

namespace CyclistDetectionStore.Publishers;

public interface ICyclistDetectionPublisher
{
    Task PublishCyclistRequestAsync(Guid intersectionId, int cyclistCount, double avgSpeed, DateTime timestamp);
    Task PublishAuditLogAsync(string message);
    Task PublishErrorLogAsync(string message, Exception exception);
}