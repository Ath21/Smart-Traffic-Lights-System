using System;
using DetectionData.TimeSeriesObjects;

namespace VehicleDetectionStore.Repositories;

public interface IVehicleDetectionRepository
{
    public Task<Guid> InsertAsync(VehicleDetection detection);
    public Task<List<VehicleDetection>> QueryAsync(
        Guid? intersectionId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? limit = null);
}
