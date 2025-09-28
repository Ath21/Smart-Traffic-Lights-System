using System;
using DetectionData;
using DetectionData.Collections.Detection;
using MongoDB.Driver;

namespace DetectionData.Repositories.EmergencyVehicle;

public class EmergencyVehicleDetectionRepository : IEmergencyVehicleDetectionRepository
{
    private readonly DetectionDbContext _context;

    public EmergencyVehicleDetectionRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(EmergencyVehicleDetection entity) =>
        await _context.EmergencyVehicles.InsertOneAsync(entity);

    public async Task<List<EmergencyVehicleDetection>> GetByIntersectionAsync(int intersectionId) =>
        await _context.EmergencyVehicles.Find(x => x.IntersectionId == intersectionId).ToListAsync();
}