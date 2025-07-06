using System;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Congestion;

public interface ICongestionAlertRepository
{
    Task<List<CongestionAlert>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date);
    Task AddAsync(CongestionAlert entry);
}
