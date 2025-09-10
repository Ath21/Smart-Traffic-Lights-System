using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Light;

public class TrafficLightRepository : ITrafficLightRepository
{
    private readonly IRedisRepository _redis;
    public TrafficLightRepository(IRedisRepository redis) => _redis = redis;

    public async Task SaveAsync(TrafficLight light)
        => await _redis.SetAsync($"trafficlight:{light.LightId}", light);

    public async Task<TrafficLight?> GetAsync(Guid lightId)
        => await _redis.GetAsync<TrafficLight>($"trafficlight:{lightId}");

    public async Task SetLightStateAsync(Guid intersectionId, Guid lightId, string state)
        => await _redis.SetHashAsync($"traffic:light:status:{intersectionId}", lightId.ToString(), state);

    public async Task<Dictionary<string, string>> GetLightStatesAsync(Guid intersectionId)
        => await _redis.GetAllHashAsync($"traffic:light:status:{intersectionId}");
}
    
