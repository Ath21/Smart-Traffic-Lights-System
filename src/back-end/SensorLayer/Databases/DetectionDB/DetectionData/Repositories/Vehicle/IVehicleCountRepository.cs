using System;
using DetectionData.Collections.Count;

namespace DetectionData.Repositories.Vehicle;

public interface IVehicleCountRepository
{
    Task AddAsync(VehicleCount record);
    Task<List<VehicleCount>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}
