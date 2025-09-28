using System;
using DetectionData.Collections.Count;

namespace DetectionData.Repositories.Vehicle;

public interface IVehicleCountRepository
{
    Task InsertAsync(VehicleCount entity);
    Task<List<VehicleCount>> GetByIntersectionAsync(int intersectionId);
}
