using System;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Cyclist;

public interface ICyclistCountRepository
{
    Task<List<CyclistCount>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date);
    Task AddAsync(CyclistCount entry);
}
