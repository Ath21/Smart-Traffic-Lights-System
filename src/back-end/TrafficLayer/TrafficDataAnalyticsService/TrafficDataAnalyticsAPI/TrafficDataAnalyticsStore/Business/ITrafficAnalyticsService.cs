using System;
using TrafficDataAnalyticsStore.Models.Dtos;

namespace TrafficDataAnalyticsStore.Business;

public interface ITrafficAnalyticsService
{
    Task<CongestionDto?> GetCurrentCongestionAsync(Guid intersectionId);
    Task<IEnumerable<IncidentDto>> GetIncidentsAsync(Guid intersectionId);
    Task<SummaryDto?> GetDailySummaryAsync(Guid intersectionId, DateTime date);
    Task<IEnumerable<SummaryDto>> GetDailyReportsAsync();
}