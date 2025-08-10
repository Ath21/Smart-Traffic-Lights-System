using System;
using DetectionData.TimeSeriesObjects;

namespace CyclistDetectionStore.Repositories;

public interface ICyclistDetectionRepository
{
    Task<Guid> InsertAsync(CyclistDetection detection);
        
    Task<List<CyclistDetection>> QueryAsync(
        Guid? intersectionId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? limit = null);
}

