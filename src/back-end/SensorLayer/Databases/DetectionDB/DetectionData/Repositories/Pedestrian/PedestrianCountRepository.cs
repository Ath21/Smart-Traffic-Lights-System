using System;
using DetectionData;
using DetectionData.Collection.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Pedestrian;

public class PedestrianCountRepository : IPedestrianCountRepository
{
    private readonly DetectionDbContext _context;

    public PedestrianCountRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PedestrianCount record) =>
        await _context.PedestrianCounts.InsertOneAsync(record);

    public async Task<List<PedestrianCount>> GetHistoryAsync(Guid intersectionId, int limit = 50)
    {
        return await _context.PedestrianCounts
            .Find(r => r.IntersectionId == intersectionId)
            .SortByDescending(r => r.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
