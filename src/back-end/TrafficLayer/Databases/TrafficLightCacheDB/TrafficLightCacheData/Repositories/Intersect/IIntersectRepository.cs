using System;
using TrafficLightCacheData.Entities;

namespace TrafficLightCacheData.Repositories.Intersect;

public interface IIntersectRepository
{
    Task SaveAsync(Intersection intersection);
    Task<Intersection?> GetAsync(string name);
}