using System;
using DetectionCacheData;
using DetectionCacheData.Entities;
using Newtonsoft.Json;

namespace DetectionCacheData.Repositories.Cache;

public class SensorCacheRepository : ISensorCacheRepository
{
    private readonly DetectionCacheDbContext _context;

    public SensorCacheRepository(DetectionCacheDbContext context)
    {
        _context = context;
    }

    public async Task<DetectionCache?> GetSnapshotAsync(Guid intersectionId)
    {
        var key = $"sensor:{intersectionId}:snapshot";
        var value = await _context.Database.StringGetAsync(key);

        if (value.IsNullOrEmpty) return null;

        return JsonConvert.DeserializeObject<DetectionCache>(value!);
    }

    public async Task SetSnapshotAsync(Guid intersectionId, DetectionCache snapshot)
    {
        var key = $"sensor:{intersectionId}:snapshot";
        var value = JsonConvert.SerializeObject(snapshot);

        await _context.Database.StringSetAsync(key, value);
    }
}
