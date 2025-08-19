using System;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorStore.Repositories.Intersections;

public interface IIntersectionRepository
{
    Task<Intersection?> GetAsync(Guid id, CancellationToken ct);
}