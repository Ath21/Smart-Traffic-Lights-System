using System;
using IntersectionControllerStore.Models.Dtos;

namespace IntersectionControllerStore.Business.TrafficConfig;

public interface ITrafficConfigurationService
{
    Task<TrafficConfigurationDto?> GetConfigurationAsync(Guid configId);
    Task<IEnumerable<TrafficConfigurationDto>> GetByIntersectionAsync(Guid intersectionId);
}
