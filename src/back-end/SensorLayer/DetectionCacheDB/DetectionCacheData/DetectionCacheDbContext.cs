using StackExchange.Redis;

namespace DetectionCacheData;

public class DetectionCacheDbContext : IDisposable
{
    private readonly ConnectionMultiplexer _connection;
    public IDatabase Database { get; }

    public DetectionCacheDbContext(RedisDbSettings settings)
    {
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{settings}:{settings.Port}" },
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