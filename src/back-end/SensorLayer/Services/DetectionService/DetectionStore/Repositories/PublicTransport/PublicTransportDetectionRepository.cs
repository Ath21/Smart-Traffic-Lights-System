using System;
using DetectionData;
using DetectionData.Collection.Detection;
using MongoDB.Driver;

namespace DetectionStore.Repositories.PublicTransport;

public class PublicTransportDetectionRepository
{
    private readonly DetectionDbContext _context;

    public PublicTransportDetectionRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task<PublicTransportDetection?> GetLatestAsync(Guid intersectionId)
    {
        return await _context.PublicTransports.Find(d => d.IntersectionId == intersectionId)
            .SortByDescending(d => d.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PublicTransportDetection detection)
    {
        await _context.PublicTransports.InsertOneAsync(detection);
    }

    public async Task<List<PublicTransportDetection>> GetHistoryAsync(Guid intersectionId, int limit = 50)
    {
        return await _context.PublicTransports.Find(d => d.IntersectionId == intersectionId)
            .SortByDescending(d => d.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
