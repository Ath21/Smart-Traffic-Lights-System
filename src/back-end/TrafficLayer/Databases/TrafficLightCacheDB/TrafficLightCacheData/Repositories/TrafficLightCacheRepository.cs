using System;
using TrafficLightCacheData.Keys;

namespace TrafficLightCacheData.Repositories;

public class TrafficLightCacheRepository : ITrafficLightCacheRepository
{
    private readonly TrafficLightCacheDbContext _context;

    public TrafficLightCacheRepository(TrafficLightCacheDbContext context)
    {
        _context = context;
    }

    public async Task SetStateAsync(int intersection, string state) =>
        await _context.SetValueAsync(TrafficLightCacheKeys.State(intersection), state);

    public async Task<string?> GetStateAsync(int intersection) =>
        await _context.GetValueAsync(TrafficLightCacheKeys.State(intersection));

    public async Task SetDurationAsync(int intersection, int seconds) =>
        await _context.SetValueAsync(TrafficLightCacheKeys.Duration(intersection), seconds.ToString());

    public async Task<int?> GetDurationAsync(int intersection)
    {
        var value = await _context.GetValueAsync(TrafficLightCacheKeys.Duration(intersection));
        return value is null ? null : int.Parse(value);
    }

    public async Task SetLastUpdateAsync(int intersection, DateTime timestamp) =>
        await _context.SetValueAsync(TrafficLightCacheKeys.LastUpdate(intersection), timestamp.ToString("o"));

    public async Task<DateTime?> GetLastUpdateAsync(int intersection)
    {
        var value = await _context.GetValueAsync(TrafficLightCacheKeys.LastUpdate(intersection));
        return value is null ? null : DateTime.Parse(value);
    }

    public async Task SetPriorityAsync(int intersection, string priority) =>
        await _context.SetValueAsync(TrafficLightCacheKeys.Priority(intersection), priority);

    public async Task<string?> GetPriorityAsync(int intersection) =>
        await _context.GetValueAsync(TrafficLightCacheKeys.Priority(intersection));

    public async Task SetQueueLengthAsync(int intersection, int length) =>
        await _context.SetValueAsync(TrafficLightCacheKeys.QueueLength(intersection), length.ToString());

    public async Task<int?> GetQueueLengthAsync(int intersection)
    {
        var value = await _context.GetValueAsync(TrafficLightCacheKeys.QueueLength(intersection));
        return value is null ? null : int.Parse(value);
    }
}