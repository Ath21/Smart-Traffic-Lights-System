using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorStore.Repositories.Intersections;

public interface IIntersectionRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<Intersection?> GetAsync(Guid id, CancellationToken ct);
}