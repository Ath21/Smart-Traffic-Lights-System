using System;
using DetectionData;
using DetectionData.Collections.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Cyclist;

public class CyclistCountRepository : ICyclistCountRepository
{
    private readonly DetectionDbContext _context;

    public CyclistCountRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(CyclistCount entity) =>
        await _context.CyclistCounts.InsertOneAsync(entity);

    public async Task<List<CyclistCount>> GetByIntersectionAsync(int intersectionId) =>
        await _context.CyclistCounts.Find(x => x.IntersectionId == intersectionId).ToListAsync();
}
