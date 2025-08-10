using System;

namespace IntersectionControlStore.Publishers.PriorityPub;

public interface IPriorityPublisher
{
    Task PublishPriorityEmergencyVehicleAsync(string intersectionId, bool priority, DateTime updatedAt);
    Task PublishPriorityPublicTransportAsync(string intersectionId, bool priority, DateTime updatedAt);
    Task PublishPriorityPedestrianAsync(string intersectionId, bool priority, DateTime updatedAt);
    Task PublishPriorityCyclistAsync(string intersectionId, bool priority, DateTime updatedAt);
}

