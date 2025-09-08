using System;
using DetectionData;
using DetectionData.Collection.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Cyclist;

public class CyclistCountRepository : ICyclistCountRepository
{
    private readonly DetectionDbContext _context;

    public CyclistCountRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CyclistCount record) =>
        await _context.CyclistCounts.InsertOneAsync(record);

    public async Task<List<CyclistCount>> GetHistoryAsync(Guid intersectionId, int limit = 50)
    {
        return await _context.CyclistCounts
            .Find(r => r.IntersectionId == intersectionId)
            .SortByDescending(r => r.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
