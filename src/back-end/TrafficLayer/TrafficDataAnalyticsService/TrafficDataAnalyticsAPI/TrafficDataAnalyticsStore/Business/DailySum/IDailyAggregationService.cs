using System;

namespace TrafficDataAnalyticsStore.Business.DailySum;

public interface IDailyAggregationService
{
    Task GenerateSummaryAsync(string intersectionId, DateTime date);
}
