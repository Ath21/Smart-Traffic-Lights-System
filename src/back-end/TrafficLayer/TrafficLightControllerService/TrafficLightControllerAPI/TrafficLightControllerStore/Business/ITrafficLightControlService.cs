using System;
using TrafficLightControllerStore.Models.Dtos;

namespace TrafficLightControllerStore.Business;

public interface ITrafficLightControlService
{
    Task<TrafficLightDto> ForceStateChangeAsync(Guid intersectionId, Guid lightId, string newState);
    Task<IEnumerable<TrafficLightDto>> GetCurrentStatesAsync(Guid intersectionId);
        // NEW
    Task<IEnumerable<ControlEventDto>> GetLastControlEventsAsync(Guid intersectionId);
}
