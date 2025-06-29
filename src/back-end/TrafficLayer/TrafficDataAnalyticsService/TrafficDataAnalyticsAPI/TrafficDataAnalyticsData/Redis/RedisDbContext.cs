using System;
using StackExchange.Redis;

namespace TrafficDataAnalyticsData.Redis;

public class RedisDbContext
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisDbContext(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public IDatabase Database => _connectionMultiplexer.GetDatabase();

    public IServer GetServer(RedisDbSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        return _connectionMultiplexer.GetServer(settings.Host, settings.Port);
    }

    public IConnectionMultiplexer ConnectionMultiplexer => _connectionMultiplexer;
}
