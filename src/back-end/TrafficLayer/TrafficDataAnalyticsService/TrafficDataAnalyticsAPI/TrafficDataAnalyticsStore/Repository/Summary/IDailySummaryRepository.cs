using System;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Summary;

public interface IDailySummaryRepository
{
    Task<List<DailySummary>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date);
    Task<List<DailySummary>> GetRangeByIntersectionAsync(string intersectionId, DateTime from, DateTime to);
    Task AddAsync(DailySummary entry);
}
