using System;
using DetectionData.TimeSeriesObjects;

namespace EmergencyVehicleDetectionStore.Repositories;

public interface IEmergencyVehicleDetectionRepository
{
    public Task<Guid> InsertAsync(EmergencyVehicleDetection emergencyVehicleDetection);
    public Task<List<EmergencyVehicleDetection>> QueryAsync(
        Guid? intersectionId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? limit = null);
}
