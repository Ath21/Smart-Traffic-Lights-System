using System;
using DetectionData.Collection.Detection;

namespace DetectionData.Repositories.EmergencyVehicle;

public interface IEmergencyVehicleDetectionRepository
{
    Task<EmergencyVehicleDetection?> GetLatestAsync(Guid intersectionId);
    Task AddAsync(EmergencyVehicleDetection detection);
    Task<List<EmergencyVehicleDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}