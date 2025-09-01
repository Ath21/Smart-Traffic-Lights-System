using System;

namespace IntersectionControlStore.Publishers.PriorityPub;

public interface IPriorityPublisher
{
    Task PublishPriorityAsync(Guid intersectionId, string priorityType, Guid? detectionId, string? reason);
}

