using System;
using IntersectionControllerData.Entities;

namespace IntersectionControllerStore.Repository.Config;

public interface ITrafficConfigurationRepository
{
    Task SaveAsync(TrafficConfiguration config);
    Task<TrafficConfiguration?> GetAsync(Guid configId);
}
