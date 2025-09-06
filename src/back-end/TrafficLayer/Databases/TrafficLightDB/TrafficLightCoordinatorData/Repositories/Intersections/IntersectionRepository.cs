using System;
using Microsoft.EntityFrameworkCore;
using TrafficLightCoordinatorData;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorStore.Repositories.Intersections;

public class IntersectionRepository : IIntersectionRepository
{
    private readonly TrafficLightCoordinatorDbContext _db;
    public IntersectionRepository(TrafficLightCoordinatorDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
        _db.Intersections.AsNoTracking().AnyAsync(i => i.IntersectionId == id, ct);

    public Task<Intersection?> GetAsync(Guid id, CancellationToken ct) =>
        _db.Intersections.AsNoTracking().FirstOrDefaultAsync(i => i.IntersectionId == id, ct);
}