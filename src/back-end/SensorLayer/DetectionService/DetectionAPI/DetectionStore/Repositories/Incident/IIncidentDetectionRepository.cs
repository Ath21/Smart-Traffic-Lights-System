using System;
using DetectionData.Collection.Detection;

namespace DetectionStore.Repositories.Incident;

public interface IIncidentDetectionRepository
{
    Task<IncidentDetection?> GetLatestAsync(Guid intersectionId);
    Task AddAsync(IncidentDetection incident);
    Task<List<IncidentDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}