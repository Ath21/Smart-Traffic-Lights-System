using System;
using Microsoft.EntityFrameworkCore;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Intersections;

public class IntersectionRepository : IIntersectionRepository
{
    private readonly TrafficLightDbContext _db;
    public IntersectionRepository(TrafficLightDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
        _db.Intersections.AsNoTracking().AnyAsync(i => i.IntersectionId == id, ct);

    public Task<Intersection?> GetAsync(Guid id, CancellationToken ct) =>
        _db.Intersections.AsNoTracking().FirstOrDefaultAsync(i => i.IntersectionId == id, ct);
}