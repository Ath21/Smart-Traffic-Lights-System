using System;
using DetectionData.Collection.Detection;

namespace DetectionStore.Repositories.PublicTransport;

public interface IPublicTransportDetectionRepository
{
    Task<PublicTransportDetection?> GetLatestAsync(Guid intersectionId);
    Task AddAsync(PublicTransportDetection detection);
    Task<List<PublicTransportDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}
