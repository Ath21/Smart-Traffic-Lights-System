using System;
using DetectionData;
using DetectionData.Collections.Detection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DetectionData.Repositories.Incident;

public class IncidentDetectionRepository : BaseRepository<IncidentDetectionCollection>, IIncidentDetectionRepository
{
    public IncidentDetectionRepository(DetectionDbContext context, ILogger<IncidentDetectionRepository> logger)
        : base(context.IncidentDetections, logger) { }

    public async Task<IEnumerable<IncidentDetectionCollection>> GetRecentIncidentsAsync(int intersectionId, int limit = 20)
    {
        var filter = Builders<IncidentDetectionCollection>.Filter.Eq(x => x.IntersectionId, intersectionId);
        return await _collection.Find(filter)
            .SortByDescending(x => x.ReportedAt)
            .Limit(limit)
            .ToListAsync();
    }
}
