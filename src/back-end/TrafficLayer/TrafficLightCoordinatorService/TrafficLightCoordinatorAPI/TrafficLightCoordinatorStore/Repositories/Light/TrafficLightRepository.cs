using System;
using Microsoft.EntityFrameworkCore;
using TrafficLightCoordinatorData;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorStore.Repositories.Light;

public class TrafficLightRepository : ITrafficLightRepository
{
    private readonly TrafficLightCoordinatorDbContext _db;
    public TrafficLightRepository(TrafficLightCoordinatorDbContext db) => _db = db;

    public Task<List<TrafficLight>> GetByIntersectionAsync(Guid intersectionId, CancellationToken ct) =>
        _db.TrafficLights.AsNoTracking()
            .Where(l => l.IntersectionId == intersectionId)
            .ToListAsync(ct);
}
