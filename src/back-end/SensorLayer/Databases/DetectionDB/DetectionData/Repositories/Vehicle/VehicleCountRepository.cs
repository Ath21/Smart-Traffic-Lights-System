using System;
using DetectionData;
using DetectionData.Collections.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Vehicle;


public class VehicleCountRepository : Repository<VehicleCount>, IVehicleCountRepository
{
    public VehicleCountRepository(DetectionDbContext context)
        : base(context.VehicleCounts) { }
}