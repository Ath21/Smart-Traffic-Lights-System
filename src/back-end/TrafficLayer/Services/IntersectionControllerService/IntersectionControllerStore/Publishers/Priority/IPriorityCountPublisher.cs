using System;
using Messages.Traffic;

namespace IntersectionControllerStore.Publishers.Priority;

public interface IPriorityCountPublisher
{
    Task PublishCountAsync(string type, int totalCount, int priorityLevel, bool thresholdExceeded,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null);
}
