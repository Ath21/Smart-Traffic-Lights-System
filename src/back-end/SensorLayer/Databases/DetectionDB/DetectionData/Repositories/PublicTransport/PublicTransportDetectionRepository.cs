using DetectionData;
using DetectionData.Collections.Detection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DetectionData.Repositories.PublicTransport;

public class PublicTransportDetectionRepository : BaseRepository<PublicTransportDetectionCollection>, IPublicTransportDetectionRepository
{
    public PublicTransportDetectionRepository(DetectionDbContext context, ILogger<PublicTransportDetectionRepository> logger)
        : base(context.PublicTransportDetections, logger) { }

    public async Task<IEnumerable<PublicTransportDetectionCollection>> GetRecentPublicTransportsAsync(int intersectionId, int limit = 50)
    {
        var filter = Builders<PublicTransportDetectionCollection>.Filter.Eq(x => x.IntersectionId, intersectionId);

        return await _collection.Find(filter)
            .SortByDescending(x => x.DetectedAt)
            .Limit(limit)
            .ToListAsync();
    }
}
