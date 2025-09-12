using System;
using DetectionData;
using DetectionData.Collections.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Vehicle;

public class VehicleCountRepository : IVehicleCountRepository
{
    private readonly DetectionDbContext _context;

    public VehicleCountRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(VehicleCount record) =>
        await _context.VehicleCounts.InsertOneAsync(record);

    public async Task<List<VehicleCount>> GetHistoryAsync(Guid intersectionId, int limit = 50)
    {
        return await _context.VehicleCounts
            .Find(r => r.IntersectionId == intersectionId)
            .SortByDescending(r => r.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
