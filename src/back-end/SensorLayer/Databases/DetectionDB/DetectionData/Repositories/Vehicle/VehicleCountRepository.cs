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

    public async Task InsertAsync(VehicleCount entity) =>
        await _context.VehicleCounts.InsertOneAsync(entity);

    public async Task<List<VehicleCount>> GetByIntersectionAsync(int intersectionId) =>
        await _context.VehicleCounts.Find(x => x.IntersectionId == intersectionId).ToListAsync();
}