using System;
using StackExchange.Redis;

namespace TrafficLightData;

public class TrafficLightDbMemoryContext
{
    private readonly ConnectionMultiplexer _connection;
    public IDatabase Database { get; }

    public TrafficLightDbMemoryContext(RedisSettings settings)
    {
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{settings.Host}:{settings.Port}" },
            AbortOnConnectFail = false,
            DefaultDatabase = settings.Database
        };

        if (!string.IsNullOrEmpty(settings.Password))
            configOptions.Password = settings.Password;

        _connection = ConnectionMultiplexer.Connect(configOptions);
        Database = _connection.GetDatabase();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
