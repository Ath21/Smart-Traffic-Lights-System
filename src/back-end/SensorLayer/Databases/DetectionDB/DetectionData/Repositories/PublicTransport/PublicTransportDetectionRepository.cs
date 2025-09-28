using System;
using DetectionData;
using DetectionData.Collections.Detection;
using MongoDB.Driver;

namespace DetectionData.Repositories.PublicTransport;

public class PublicTransportDetectionRepository : IPublicTransportDetectionRepository
{
    private readonly DetectionDbContext _context;

    public PublicTransportDetectionRepository(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(PublicTransportDetection entity) =>
        await _context.PublicTransports.InsertOneAsync(entity);

    public async Task<List<PublicTransportDetection>> GetByIntersectionAsync(int intersectionId) =>
        await _context.PublicTransports.Find(x => x.IntersectionId == intersectionId).ToListAsync();
}
