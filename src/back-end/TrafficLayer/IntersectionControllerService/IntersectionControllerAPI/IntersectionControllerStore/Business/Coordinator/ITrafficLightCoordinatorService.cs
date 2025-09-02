using System;

namespace IntersectionControllerStore.Business.Coordinator;

public interface ITrafficLightCoordinatorService
{
    Task ApplyUpdateAsync(Guid intersectionId, Guid lightId, string newState, DateTime updatedAt);
}