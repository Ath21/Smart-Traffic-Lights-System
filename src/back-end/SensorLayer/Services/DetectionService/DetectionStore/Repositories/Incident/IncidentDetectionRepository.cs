using System;
using DetectionData;
using DetectionData.Collection.Detection;
using MongoDB.Driver;

namespace DetectionStore.Repositories.Incident;

public class IncidentDetectionRepository
{
    private readonly DetectionDbContext _context;

    public IncidentDetectionRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task<IncidentDetection?> GetLatestAsync(Guid intersectionId)
    {
        return await _context.Incidents.Find(d => d.IntersectionId == intersectionId)
            .SortByDescending(d => d.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(IncidentDetection incident)
    {
        await _context.Incidents.InsertOneAsync(incident);
    }

    public async Task<List<IncidentDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50)
    {
        return await _context.Incidents.Find(d => d.IntersectionId == intersectionId)
            .SortByDescending(d => d.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
