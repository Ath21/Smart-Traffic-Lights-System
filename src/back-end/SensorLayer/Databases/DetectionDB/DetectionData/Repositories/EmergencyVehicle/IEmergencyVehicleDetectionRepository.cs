using System;
using DetectionData.Collections.Detection;

namespace DetectionData.Repositories.EmergencyVehicle;

public interface IEmergencyVehicleDetectionRepository
{
    Task InsertAsync(EmergencyVehicleDetection entity);
    Task<List<EmergencyVehicleDetection>> GetByIntersectionAsync(int intersectionId);
}