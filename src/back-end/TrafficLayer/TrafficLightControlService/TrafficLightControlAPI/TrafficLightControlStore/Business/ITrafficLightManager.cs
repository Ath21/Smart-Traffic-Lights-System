using System;
using TrafficMessages.Light;

namespace TrafficLightControlStore.Business;

public interface ITrafficLightManager
{
    Task ApplyControlAsync(TrafficLightControl control);
    Task OverrideLightAsync(Guid lightId, string state, int durationSeconds, string triggeredBy);
    (string State, DateTime UpdatedAt)? GetLightState(Guid lightId);
}
