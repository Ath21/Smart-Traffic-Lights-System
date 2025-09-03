using System;
using IntersectionControllerData;
using StackExchange.Redis;

namespace TrafficLightControlStore.Repository;

public class TrafficLightRepository : ITrafficLightRepository
{
    private readonly IDatabase _db;

    public TrafficLightRepository(TrafficLightDbMemoryContext context)
    {
        _db = context.Database;
    }

    private static string LightKey(Guid intersectionId, Guid lightId) =>
        $"traffic:light:{intersectionId}:{lightId}:state";

    private static string ControlKey(Guid intersectionId, Guid lightId) =>
        $"traffic:light:{intersectionId}:{lightId}:control";

    public async Task SetLightStateAsync(Guid intersectionId, Guid lightId, string state) =>
        await _db.StringSetAsync(LightKey(intersectionId, lightId), state);

    public async Task<string?> GetLightStateAsync(Guid intersectionId, Guid lightId) =>
        await _db.StringGetAsync(LightKey(intersectionId, lightId));

    public async Task SaveControlEventAsync(Guid intersectionId, Guid lightId, string command)
    {
        var key = ControlKey(intersectionId, lightId);
        var value = $"{command}:{DateTime.UtcNow:o}";
        await _db.StringSetAsync(key, value);
    }

    public async Task<string?> GetLastControlEventAsync(Guid intersectionId, Guid lightId) =>
        await _db.StringGetAsync(ControlKey(intersectionId, lightId));
}
