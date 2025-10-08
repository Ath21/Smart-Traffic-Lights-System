using System;
using DetectionData.Collections.Detection;

namespace DetectionData.Repositories.PublicTransport;

public interface IPublicTransportDetectionRepository
{
    Task<IEnumerable<PublicTransportDetectionCollection>> GetByLineAsync(string lineName);
    Task InsertAsync(PublicTransportDetectionCollection entity);
}