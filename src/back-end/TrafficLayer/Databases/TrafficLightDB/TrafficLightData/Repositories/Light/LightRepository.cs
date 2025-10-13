using System;
using Microsoft.EntityFrameworkCore;

using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Light;

public class LightRepository : ILightRepository
{
    private readonly TrafficLightDbContext _db;
    public LightRepository(TrafficLightDbContext db) => _db = db;

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
