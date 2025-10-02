using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace TrafficLightCacheData;

public class TrafficLightCacheDbContext : IDisposable
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

    public void Dispose() => _connection.Dispose();

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            var pong = await Database.PingAsync();
            return pong != TimeSpan.Zero;
        }
        catch
        {
            return false;
        }
    }

    // Helper methods
    public async Task SetValueAsync(string key, string value, TimeSpan? expiry = null) =>
        await Database.StringSetAsync(key, value, expiry);

    public async Task<string?> GetValueAsync(string key) =>
        await Database.StringGetAsync(key);

    public async Task<long> IncrementAsync(string key, long value = 1) =>
        await Database.StringIncrementAsync(key, value);

    public async Task<bool> KeyExistsAsync(string key) =>
        await Database.KeyExistsAsync(key);
}
