using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Config;

public interface ITrafficConfigRepository
{
    Task SaveAsync(TrafficConfiguration config);
    Task<TrafficConfiguration?> GetAsync(Guid configId);
}
