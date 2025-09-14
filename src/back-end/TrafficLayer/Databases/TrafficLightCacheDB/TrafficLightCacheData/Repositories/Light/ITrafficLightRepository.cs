using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Light;

public interface ITrafficLightRepository
{
    Task SaveAsync(TrafficLight light);
    Task<TrafficLight?> GetAsync(string intersection, string light);

    Task SetLightStateAsync(string intersection, string light, string state);
    Task<string?> GetLightStateAsync(string intersection, string light);

    Task SetOverrideAsync(string intersection, string light, string state, int duration, string? reason, DateTime? expiresAt);
    Task<Dictionary<string, string>> GetAllStatesAsync(string intersection);
}