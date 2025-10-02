using System;
using System.Text.Json;
using TrafficLightData.Entities;

namespace TrafficLightData.Repositories.TrafficConfig;

public interface ITrafficConfigurationRepository : IRepository<TrafficConfiguration>
{
    Task<IEnumerable<TrafficConfiguration>> GetByIntersectionAsync(int intersectionId);
}