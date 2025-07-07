using System;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Congestion;

public interface ICongestionAlertRepository
{
    Task<List<CongestionAlert>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date);
    Task<List<CongestionAlert>> GetRecentAlertsAsync(string severity, int limit);
    Task AddAsync(CongestionAlert entry);
}
