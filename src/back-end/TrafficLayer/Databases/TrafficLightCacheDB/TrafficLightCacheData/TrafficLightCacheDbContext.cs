using System;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData;

public class TrafficLightCacheDbContext
{
    private readonly ConnectionMultiplexer _connection;
    public IDatabase Database { get; }

    public TrafficLightCacheDbContext(IOptions<TrafficLightCacheDbSettings> settings)
    {
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{settings.Value.Host}:{settings.Value.Port}" },
            AbortOnConnectFail = false,
            DefaultDatabase = settings.Value.Database
        };

        if (!string.IsNullOrEmpty(settings.Value.Password))
            configOptions.Password = settings.Value.Password;

        _connection = ConnectionMultiplexer.Connect(configOptions);
        Database = _connection.GetDatabase();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
