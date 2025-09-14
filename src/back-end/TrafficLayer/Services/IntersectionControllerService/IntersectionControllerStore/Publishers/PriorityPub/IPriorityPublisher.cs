using System;

namespace IntersectionControllerStore.Publishers.PriorityPub;

public interface IPriorityPublisher
{
    Task PublishPriorityAsync(string intersection, string type, string? detectionId, string? reason);
}

