using System;
using IntersectionControllerData.Entities;

namespace IntersectionControllerStore.Repository.Intersect;

public class IntersectionRepository : IIntersectionRepository
{
    private readonly IRedisRepository _redis;
    public IntersectionRepository(IRedisRepository redis) => _redis = redis;

    public async Task SaveAsync(Intersection intersection)
        => await _redis.SetAsync($"intersection:{intersection.IntersectionId}", intersection);

    public async Task<Intersection?> GetAsync(Guid intersectionId)
        => await _redis.GetAsync<Intersection>($"intersection:{intersectionId}");
}
