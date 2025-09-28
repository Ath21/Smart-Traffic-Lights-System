using System;
using DetectionData;
using DetectionData.Collections.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Pedestrian;

public class PedestrianCountRepository : IPedestrianCountRepository
{
    private readonly DetectionDbContext _context;

    public PedestrianCountRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(PedestrianCount entity) =>
        await _context.PedestrianCounts.InsertOneAsync(entity);

    public async Task<List<PedestrianCount>> GetByIntersectionAsync(int intersectionId) =>
        await _context.PedestrianCounts.Find(x => x.IntersectionId == intersectionId).ToListAsync();
}
