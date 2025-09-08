using System;

namespace TrafficLightCacheData.Repositories;

public interface IRedisRepository
{
    Task SetAsync<T>(string key, T value);
    Task<T?> GetAsync<T>(string key);
    Task PushToListAsync<T>(string key, T value);
    Task<List<T>> GetListAsync<T>(string key, int count = 20);
    Task SetHashAsync(string key, string field, string value);
    Task<Dictionary<string, string>> GetAllHashAsync(string key);
}

