using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Config;

public class TrafficConfigRepository : ITrafficConfigRepository
{
    private readonly IRedisRepository _redis;
    public TrafficConfigRepository(IRedisRepository redis) => _redis = redis;

    public async Task SaveAsync(TrafficConfiguration config)
        => await _redis.SetAsync($"traffic_config:{config.Name}", config);

    public async Task<TrafficConfiguration?> GetAsync(string name)
        => await _redis.GetAsync<TrafficConfiguration>($"traffic_config:{name}");
}
