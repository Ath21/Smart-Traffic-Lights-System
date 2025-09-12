using System;
using DetectionData.Collections.Detection;

namespace DetectionData.Repositories.Incident;

public interface IIncidentDetectionRepository
{
    Task<IncidentDetection?> GetLatestAsync(Guid intersectionId);
    Task AddAsync(IncidentDetection incident);
    Task<List<IncidentDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}