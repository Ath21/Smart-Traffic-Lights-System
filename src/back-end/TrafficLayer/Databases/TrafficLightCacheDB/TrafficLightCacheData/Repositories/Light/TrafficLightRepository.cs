using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Light;

public class TrafficLightRepository : ITrafficLightRepository
{
    private readonly IRedisRepository _redis;
    public TrafficLightRepository(IRedisRepository redis) => _redis = redis;

    // Save full entity for backup/debug
    public async Task SaveAsync(TrafficLight light)
        => await _redis.SetAsync($"traffic_light:{light.Intersection}.{light.Light}", light);

    public async Task<TrafficLight?> GetAsync(string intersection, string light)
        => await _redis.GetAsync<TrafficLight>($"traffic_light:{intersection}.{light}");

    // Set state in Redis (normal cycle or override)
    public async Task SetLightStateAsync(string intersection, string light, string state)
        => await _redis.SetAsync($"traffic_light:{intersection}.{light}:state", state);

    public async Task<string?> GetLightStateAsync(string intersection, string light)
        => await _redis.GetAsync<string>($"traffic_light:{intersection}.{light}:state");

    // Store manual override with duration + reason
    public async Task SetOverrideAsync(string intersection, string light, string state, int duration, string? reason, DateTime? expiresAt)
    {
        var prefix = $"traffic_light:{intersection}.{light}:override";
        await _redis.SetAsync($"{prefix}:state", state);
        await _redis.SetAsync($"{prefix}:duration", duration);
        if (!string.IsNullOrWhiteSpace(reason))
            await _redis.SetAsync($"{prefix}:reason", reason);
        if (expiresAt.HasValue)
            await _redis.SetAsync($"{prefix}:expires", expiresAt.Value.ToString("O")); // ISO 8601
    }

    public async Task<Dictionary<string, string>> GetAllStatesAsync(string intersection)
        => await _redis.GetAllHashAsync($"traffic_light:{intersection}:states");
}
