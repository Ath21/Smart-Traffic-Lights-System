using System;
using DetectionData;
using DetectionData.Collections.Detection;
using MongoDB.Driver;

namespace DetectionData.Repositories.Incident;

public class IncidentDetectionRepository : IIncidentDetectionRepository
{
    private readonly DetectionDbContext _context;

    public IncidentDetectionRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(IncidentDetection entity) =>
        await _context.Incidents.InsertOneAsync(entity);

    public async Task<List<IncidentDetection>> GetByIntersectionAsync(int intersectionId) =>
        await _context.Incidents.Find(x => x.IntersectionId == intersectionId).ToListAsync();
}
