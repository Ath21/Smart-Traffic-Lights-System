using System;
using DetectionData.Collections.Detection;

namespace DetectionData.Repositories.PublicTransport;

public interface IPublicTransportDetectionRepository
{
    Task InsertAsync(PublicTransportDetection entity);
    Task<List<PublicTransportDetection>> GetByIntersectionAsync(int intersectionId);
}