using System;
using TrafficLightControllerStore.Models.Dtos;

namespace TrafficLightControllerStore.Business;

public interface ITrafficLightControlService
{
    Task<TrafficLightDto> ForceStateChangeAsync(
        string intersection,
        string light,
        string newState,
        int? duration = null,
        string? reason = null);
    Task<IEnumerable<TrafficLightDto>> GetCurrentStatesAsync(string intersection);
    Task<IEnumerable<ControlEventDto>> GetLastControlEventsAsync(string intersection);
}
