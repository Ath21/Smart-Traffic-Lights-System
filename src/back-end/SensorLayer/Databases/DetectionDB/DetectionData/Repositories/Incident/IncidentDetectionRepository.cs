using System;
using DetectionData;
using DetectionData.Collections.Detection;
using MongoDB.Driver;

namespace DetectionData.Repositories.Incident;

public class IncidentDetectionRepository : Repository<IncidentDetection>, IIncidentDetectionRepository
{
    public IncidentDetectionRepository(DetectionDbContext context)
        : base(context.Incidents) { }
}
