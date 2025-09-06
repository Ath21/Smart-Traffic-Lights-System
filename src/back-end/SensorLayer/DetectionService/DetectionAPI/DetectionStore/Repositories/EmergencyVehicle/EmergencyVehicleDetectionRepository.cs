using System;
using DetectionData;
using DetectionData.Collection.Detection;
using MongoDB.Driver;

namespace DetectionStore.Repositories.EmergencyVehicle;

public class EmergencyVehicleDetectionRepository : IEmergencyVehicleDetectionRepository
{
    private readonly DetectionDbContext _context;

    public EmergencyVehicleDetectionRepository(DetectionDbContext context, DetectionDbSettings settings)
    {
        _context = context;
    }

    public async Task<EmergencyVehicleDetection?> GetLatestAsync(Guid intersectionId)
    {
        return await _context.EmergencyVehicles.Find(d => d.IntersectionId == intersectionId)
            .SortByDescending(d => d.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(EmergencyVehicleDetection detection)
    {
        await _context.EmergencyVehicles.InsertOneAsync(detection);
    }

    public async Task<List<EmergencyVehicleDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50)
    {
        return await _context.EmergencyVehicles.Find(d => d.IntersectionId == intersectionId)
            .SortByDescending(d => d.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}