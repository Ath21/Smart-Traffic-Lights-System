using System;
using IntersectionControllerData;
using StackExchange.Redis;

namespace TrafficLightControlStore.Repository;

public class TrafficLightRepository : ITrafficLightRepository
{
    private readonly IDatabase _db;

    public TrafficLightRepository(IntersectionControllerData.TrafficLightDbMemoryContext context)
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

        // Also push to a list per intersection for history
        var listKey = $"traffic:light:{intersectionId}:events";
        await _db.ListLeftPushAsync(listKey, $"{lightId}:{command}:{DateTime.UtcNow:o}");
        await _db.ListTrimAsync(listKey, 0, 49); // keep last 50 events
    }

    public async Task<string?> GetLastControlEventAsync(Guid intersectionId, Guid lightId) =>
        await _db.StringGetAsync(ControlKey(intersectionId, lightId));

    public async Task<IEnumerable<(Guid LightId, string Command, DateTime Timestamp)>> GetControlEventsAsync(Guid intersectionId)
    {
        var listKey = $"traffic:light:{intersectionId}:events";
        var entries = await _db.ListRangeAsync(listKey, 0, 49);

        var results = entries
            .Select(e =>
            {
                var parts = e.ToString().Split(':', 3);
                if (parts.Length == 3 &&
                    Guid.TryParse(parts[0], out var lightId) &&
                    DateTime.TryParse(parts[2], out var ts))
                {
                    return (lightId, parts[1], ts);
                }
                return (Guid.Empty, "invalid", DateTime.MinValue);
            })
            .Where(tuple => tuple.Item1 != Guid.Empty); // filter on Item1 (LightId)

        return results;
    }
}