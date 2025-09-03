using System;
using TrafficLightControlStore.Models.Dtos;

namespace TrafficLightControlStore.Business;

public interface ITrafficLightControlService
{
    Task<TrafficLightDto> ForceStateChangeAsync(Guid intersectionId, Guid lightId, string newState);
    Task<IEnumerable<TrafficLightDto>> GetCurrentStatesAsync(Guid intersectionId);
}
