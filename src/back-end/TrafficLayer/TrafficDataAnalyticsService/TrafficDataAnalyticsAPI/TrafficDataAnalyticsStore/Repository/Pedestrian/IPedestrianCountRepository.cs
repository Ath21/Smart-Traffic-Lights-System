using System;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Pedestrian;

public interface IPedestrianCountRepository
{
    Task<List<PedestrianCount>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date);
    Task AddAsync(PedestrianCount entry);
}
