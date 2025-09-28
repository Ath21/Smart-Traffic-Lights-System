using System;

namespace DetectionCacheData.Repositories;

public interface IDetectionCacheRepository
{
    Task SetVehicleCountAsync(int intersection, int count);
    Task<int?> GetVehicleCountAsync(int intersection);

    Task SetPedestrianCountAsync(int intersection, int count);
    Task<int?> GetPedestrianCountAsync(int intersection);

    Task SetCyclistCountAsync(int intersection, int count);
    Task<int?> GetCyclistCountAsync(int intersection);

    Task SetEmergencyDetectedAsync(int intersection, bool detected);
    Task<bool?> GetEmergencyDetectedAsync(int intersection);

    Task SetPublicTransportDetectedAsync(int intersection, bool detected);
    Task<bool?> GetPublicTransportDetectedAsync(int intersection);

    Task SetIncidentDetectedAsync(int intersection, string jsonIncident);
    Task<string?> GetIncidentDetectedAsync(int intersection);
}
