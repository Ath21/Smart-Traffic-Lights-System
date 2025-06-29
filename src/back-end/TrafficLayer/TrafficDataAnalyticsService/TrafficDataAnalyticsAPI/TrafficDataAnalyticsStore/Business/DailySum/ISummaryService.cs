using System;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.DailySum;

public interface ISummaryService
{
    Task<List<DailySummaryDto>> GetLatestSummariesAsync();
}
