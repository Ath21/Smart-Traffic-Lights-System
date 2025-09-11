using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Config;

public class TrafficConfigRepository : ITrafficConfigRepository
{
    private readonly IRedisRepository _redis;
    public TrafficConfigRepository(IRedisRepository redis) => _redis = redis;

    public async Task SaveAsync(TrafficConfiguration config)
        => await _redis.SetAsync($"trafficconfig:{config.ConfigId}", config);

    public async Task<TrafficConfiguration?> GetAsync(Guid configId)
        => await _redis.GetAsync<TrafficConfiguration>($"trafficconfig:{configId}");
}