using System;
using DetectionData;
using DetectionData.Collections.Detection;
using MongoDB.Driver;

namespace DetectionData.Repositories.EmergencyVehicle;


public class EmergencyVehicleDetectionRepository : Repository<EmergencyVehicleDetection>, IEmergencyVehicleDetectionRepository
{
    public EmergencyVehicleDetectionRepository(DetectionDbContext context)
        : base(context.EmergencyVehicles) { }
}