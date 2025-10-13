using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Light;

public interface ITrafficLightRepository
{
    Task SaveAsync(TrafficLight light);
    Task<TrafficLight?> GetAsync(Guid lightId);
    Task SetLightStateAsync(Guid intersectionId, Guid lightId, string state);
    Task<Dictionary<string,string>> GetLightStatesAsync(Guid intersectionId);
}