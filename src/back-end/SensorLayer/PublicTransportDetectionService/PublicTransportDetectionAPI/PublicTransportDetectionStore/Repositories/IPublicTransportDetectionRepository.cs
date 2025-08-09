using System;
using DetectionData.TimeSeriesObjects;

namespace PublicTransportDetectionStore.Repositories;

public interface IPublicTransportDetectionRepository
{
    public Task<Guid> InsertAsync(PublicTransportDetection detection);
    public Task<List<PublicTransportDetection>> QueryAsync(
            Guid? intersectionId = null,
            string? routeId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null);
}
