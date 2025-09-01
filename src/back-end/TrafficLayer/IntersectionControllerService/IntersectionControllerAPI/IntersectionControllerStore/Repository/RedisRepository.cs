using System.Text.Json;
using StackExchange.Redis;
using IntersectionControllerData;

namespace IntersectionControllerStore.Repository;

public class RedisRepository : IRedisRepository
{
    private readonly IDatabase _db;

    public RedisRepository(TrafficLightDbMemoryContext context)
    {
        _db = context.Database;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public async Task PushToListAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.ListLeftPushAsync(key, json);
    }

    public async Task<List<T>> GetListAsync<T>(string key, int count = 20)
    {
        var values = await _db.ListRangeAsync(key, 0, count - 1);
        return values.Select(v => JsonSerializer.Deserialize<T>(v!)!).ToList();
    }

    public async Task SetHashAsync(string key, string field, string value)
        => await _db.HashSetAsync(key, field, value);

    public async Task<Dictionary<string, string>> GetAllHashAsync(string key)
    {
        var entries = await _db.HashGetAllAsync(key);
        return entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
    }
}
