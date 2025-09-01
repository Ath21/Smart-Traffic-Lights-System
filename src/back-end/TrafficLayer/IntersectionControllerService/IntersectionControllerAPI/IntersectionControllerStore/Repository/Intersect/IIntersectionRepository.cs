using System;
using IntersectionControllerData.Entities;

namespace IntersectionControllerStore.Repository.Intersect;

public interface IIntersectionRepository
{
    Task SaveAsync(Intersection intersection);
    Task<Intersection?> GetAsync(Guid intersectionId);
}