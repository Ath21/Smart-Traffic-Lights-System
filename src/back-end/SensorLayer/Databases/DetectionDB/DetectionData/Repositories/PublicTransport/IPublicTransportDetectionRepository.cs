using System;
using DetectionData.Collections.Detection;

namespace DetectionData.Repositories.PublicTransport;

public interface IPublicTransportDetectionRepository
{
    Task<PublicTransportDetection?> GetLatestAsync(Guid intersectionId);
    Task AddAsync(PublicTransportDetection detection);
    Task<List<PublicTransportDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}
