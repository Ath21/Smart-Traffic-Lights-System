using System;

namespace TrafficLightControlStore.Repository;

public interface ITrafficLightRepository
{
    Task SetLightStateAsync(Guid intersectionId, Guid lightId, string state);
    Task<string?> GetLightStateAsync(Guid intersectionId, Guid lightId);
    Task SaveControlEventAsync(Guid intersectionId, Guid lightId, string command);
    Task<string?> GetLastControlEventAsync(Guid intersectionId, Guid lightId);

    // NEW: fetch all events for an intersection
    Task<IEnumerable<(Guid LightId, string Command, DateTime Timestamp)>> GetControlEventsAsync(Guid intersectionId);
}