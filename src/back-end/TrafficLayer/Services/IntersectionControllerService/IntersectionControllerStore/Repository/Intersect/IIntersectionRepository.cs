using System;
using TrafficLightData.Entities;

namespace IntersectionControllerStore.Repository.Intersect;

public interface IIntersectionRepository
{
    Task SaveAsync(Intersection intersection);
    Task<Intersection?> GetAsync(Guid intersectionId);
}