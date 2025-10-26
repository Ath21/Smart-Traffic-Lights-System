using System;
using DetectionData;
using DetectionData.Collections.Count;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DetectionData.Repositories.Cyclist;

public class CyclistCountRepository : BaseRepository<CyclistCountCollection>, ICyclistCountRepository
{
    public CyclistCountRepository(DetectionDbContext context, ILogger<CyclistCountRepository> logger)
        : base(context.CyclistCount, logger) { }

    public async Task<IEnumerable<CyclistCountCollection>> GetRecentByIntersectionAsync(int intersectionId, int limit = 100)
    {
        var filter = Builders<CyclistCountCollection>.Filter.Eq(x => x.IntersectionId, intersectionId);
        var result = await _collection.Find(filter)
                                      .SortByDescending(x => x.Timestamp)
                                      .Limit(limit)
                                      .ToListAsync();
        return result;
    }
}
