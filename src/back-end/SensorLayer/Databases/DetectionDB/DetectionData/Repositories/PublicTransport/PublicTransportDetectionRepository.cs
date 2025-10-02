using System;
using DetectionData;
using DetectionData.Collections.Detection;
using MongoDB.Driver;

namespace DetectionData.Repositories.PublicTransport;

public class PublicTransportDetectionRepository : Repository<PublicTransportDetection>, IPublicTransportDetectionRepository
{
    public PublicTransportDetectionRepository(DetectionDbContext context)
        : base(context.PublicTransport) { }
}
