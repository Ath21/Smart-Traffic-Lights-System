using System;

namespace IntersectionControllerStore.Publishers.Priority;

public interface IPriorityEventPublisher
{
    Task PublishEventAsync(string eventType, string vehicleType, int priorityLevel,
        string direction, Dictionary<string, string>? metadata = null, Guid? correlationId = null);
}
