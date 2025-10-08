using System;
using DetectionData;
using DetectionData.Collections.Detection;
using MongoDB.Driver;

namespace DetectionData.Repositories.PublicTransport;

public class PublicTransportDetectionRepository : BaseRepository<PublicTransportDetectionCollection>, IPublicTransportDetectionRepository
{
    public PublicTransportDetectionRepository(DetectionDbContext context)
        : base(context.PublicTransportDetections) { }

    public async Task<IEnumerable<PublicTransportDetectionCollection>> GetByLineAsync(string lineName)
    {
        var filter = Builders<PublicTransportDetectionCollection>.Filter.Eq(x => x.LineName, lineName);
        return await _collection.Find(filter).SortByDescending(x => x.DetectedAt).ToListAsync();
    }
}
