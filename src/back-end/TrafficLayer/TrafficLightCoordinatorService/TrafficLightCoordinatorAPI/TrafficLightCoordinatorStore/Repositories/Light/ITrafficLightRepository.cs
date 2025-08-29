using System;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorStore.Repositories.Light;

public interface ITrafficLightRepository
{
    Task<List<TrafficLight>> GetByIntersectionAsync(Guid intersectionId, CancellationToken ct);
    Task<TrafficLight?> GetLatestAsync(Guid intersectionId, CancellationToken ct);
}
