using System;
using TrafficLightCacheData.Repositories;

namespace IntersectionControllerStore.Business.CommandLog;

public class CommandLogService : ICommandLogService
{
    private readonly IRedisRepository _redis;

    public CommandLogService(IRedisRepository redis)
    {
        _redis = redis;
    }

    public async Task LogCommandAsync(Guid intersectionId, object command)
    {
        var key = $"traffic:light:events:{intersectionId}";
        await _redis.PushToListAsync(key, command);
    }

    public async Task<List<T>> GetRecentCommandsAsync<T>(Guid intersectionId, int count = 20)
    {
        var key = $"traffic:light:events:{intersectionId}";
        return await _redis.GetListAsync<T>(key, count);
    }
}
