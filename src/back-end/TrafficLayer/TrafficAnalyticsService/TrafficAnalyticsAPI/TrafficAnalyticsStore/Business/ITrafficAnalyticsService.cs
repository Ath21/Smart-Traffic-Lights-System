using System;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Business;

public interface ITrafficAnalyticsService
{
    // Queries
    Task<CongestionDto?> GetCurrentCongestionAsync(Guid intersectionId);
    Task<IEnumerable<IncidentDto>> GetIncidentsAsync(Guid intersectionId);
    Task<SummaryDto?> GetDailySummaryAsync(Guid intersectionId, DateTime date);
    Task<IEnumerable<SummaryDto>> GetDailyReportsAsync();

    // Commands (now also handle publishing)
    Task AddOrUpdateSummaryAsync(SummaryDto dto);
    Task ReportIncidentAsync(IncidentDto dto);
}