using System;

namespace IntersectionControllerStore.Business.Priority;

public interface IPriorityManager
{
    Task ProcessVehicleCountAsync(Guid intersectionId, int count, float avgSpeed, DateTime timestamp);
    Task ProcessEmergencyVehicleAsync(Guid intersectionId, Guid detectionId, bool detected, DateTime timestamp);
    Task ProcessPublicTransportAsync(Guid intersectionId, Guid detectionId, string? routeId, DateTime timestamp);
    Task ProcessPedestrianAsync(Guid intersectionId, Guid detectionId, int count, DateTime timestamp);
    Task ProcessCyclistAsync(Guid intersectionId, Guid detectionId, int count, DateTime timestamp);
    Task ProcessIncidentAsync(Guid intersectionId, Guid detectionId, string description, DateTime timestamp);
}
