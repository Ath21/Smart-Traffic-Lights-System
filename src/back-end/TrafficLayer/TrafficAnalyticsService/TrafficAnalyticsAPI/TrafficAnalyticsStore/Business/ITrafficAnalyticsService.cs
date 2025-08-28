using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Business;

public interface ITrafficAnalyticsService
{
    // [GET]    /api/traffic/analytics/congestion/{intersectionId}
    Task<CongestionDto?> GetCurrentCongestionAsync(Guid intersectionId);
    // [GET]    /api/traffic/analytics/incidents/{intersectionId}
    Task<IEnumerable<IncidentDto>> GetIncidentsAsync(Guid intersectionId);
    // [GET]    /api/traffic/analytics/summary/{intersectionId}/{date}
    Task<SummaryDto?> GetDailySummaryAsync(Guid intersectionId, DateTime date);
    // [GET]    /api/traffic/analytics/reports/daily
    Task<IEnumerable<SummaryDto>> GetDailyReportsAsync();

    // Command: add or update a daily summary
    Task AddOrUpdateSummaryAsync(SummaryDto dto);

    // Command: report incident
    Task ReportIncidentAsync(IncidentDto dto);
}