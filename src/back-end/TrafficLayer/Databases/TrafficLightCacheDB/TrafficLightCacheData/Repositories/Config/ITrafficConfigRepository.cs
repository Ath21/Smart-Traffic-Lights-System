using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Config;

public interface ITrafficConfigurationRepository
{
    Task SaveAsync(TrafficConfiguration config);
    Task<TrafficConfiguration?> GetAsync(Guid configId);
}
