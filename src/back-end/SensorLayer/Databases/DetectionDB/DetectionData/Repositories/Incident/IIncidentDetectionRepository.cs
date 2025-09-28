using System;
using DetectionData.Collections.Detection;

namespace DetectionData.Repositories.Incident;

public interface IIncidentDetectionRepository
{
    Task InsertAsync(IncidentDetection entity);
    Task<List<IncidentDetection>> GetByIntersectionAsync(int intersectionId);
}