using System;
using IntersectionControllerStore.Models.Dtos;

namespace IntersectionControllerStore.Business.TrafficLight;

public interface ITrafficLightService
{
    Task<TrafficLightDto?> GetLightAsync(Guid lightId);
    Task<IEnumerable<TrafficLightDto>> GetByIntersectionAsync(Guid intersectionId);
    Task UpdateLightStateAsync(Guid intersectionId, Guid lightId, string newState);
}