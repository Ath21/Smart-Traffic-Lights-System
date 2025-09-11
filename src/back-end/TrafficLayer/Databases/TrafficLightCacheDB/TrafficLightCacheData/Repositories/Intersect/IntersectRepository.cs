using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Intersect;

public class IntersectRepository : IIntersectRepository
{
    private readonly IRedisRepository _redis;
    public IntersectRepository(IRedisRepository redis) => _redis = redis;

    public async Task SaveAsync(Intersection intersection)
        => await _redis.SetAsync($"intersection:{intersection.IntersectionId}", intersection);

    public async Task<Intersection?> GetAsync(Guid intersectionId)
        => await _redis.GetAsync<Intersection>($"intersection:{intersectionId}");
}
