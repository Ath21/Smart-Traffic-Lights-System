using System;
using TrafficLightData.Entities;

namespace IntersectionControllerStore.Repository.Config;

public interface ITrafficConfigurationRepository
{
    Task SaveAsync(TrafficConfiguration config);
    Task<TrafficConfiguration?> GetAsync(Guid configId);
}
