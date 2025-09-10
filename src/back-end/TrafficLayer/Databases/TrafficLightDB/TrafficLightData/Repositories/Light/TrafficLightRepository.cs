using System;
using Microsoft.EntityFrameworkCore;

using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Light;

public class TrafficLightRepository : ITrafficLightRepository
{
    private readonly TrafficLightDbContext _db;
    public TrafficLightRepository(TrafficLightDbContext db) => _db = db;

    public Task<List<TrafficLight>> GetByIntersectionAsync(Guid intersectionId, CancellationToken ct) =>
        _db.TrafficLights.AsNoTracking()
            .Where(l => l.IntersectionId == intersectionId)
            .OrderByDescending(l => l.UpdatedAt)
            .ToListAsync(ct);

    public Task<TrafficLight?> GetLatestAsync(Guid intersectionId, CancellationToken ct) =>
        _db.TrafficLights.AsNoTracking()
            .Where(l => l.IntersectionId == intersectionId)
            .OrderByDescending(l => l.UpdatedAt)
            .FirstOrDefaultAsync(ct);
}
