using System;
using TrafficLightData.Entities;

namespace IntersectionControllerStore.Repository.Config;

public class TrafficConfigurationRepository : ITrafficConfigurationRepository
{
    private readonly IRedisRepository _redis;
    public TrafficConfigurationRepository(IRedisRepository redis) => _redis = redis;

    public async Task SaveAsync(TrafficConfiguration config)
        => await _redis.SetAsync($"trafficconfig:{config.ConfigId}", config);

    public async Task<TrafficConfiguration?> GetAsync(Guid configId)
        => await _redis.GetAsync<TrafficConfiguration>($"trafficconfig:{configId}");
}