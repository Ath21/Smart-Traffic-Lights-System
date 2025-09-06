using System;
using DetectionCacheData;

namespace SensorStore.Repositories.Metrics;

public class MetricRepository : IMetricRepository
{
    private readonly DetectionCacheDbContext _context;

    public MetricRepository(DetectionCacheDbContext context)
    {
        _context = context;
    }

    private string BuildKey(Guid intersectionId, string metricName) =>
        $"sensor:{intersectionId}:{metricName}";

    public async Task<int> GetMetricAsync(Guid intersectionId, string metricName)
    {
        var key = BuildKey(intersectionId, metricName);
        var value = await _context.Database.StringGetAsync(key);

        return value.IsNullOrEmpty ? 0 : (int)value;
    }

    public async Task SetMetricAsync(Guid intersectionId, string metricName, int value)
    {
        var key = BuildKey(intersectionId, metricName);
        await _context.Database.StringSetAsync(key, value);
    }

    public async Task<bool> ExistsAsync(Guid intersectionId, string metricName)
    {
        var key = BuildKey(intersectionId, metricName);
        return await _context.Database.KeyExistsAsync(key);
    }
}
