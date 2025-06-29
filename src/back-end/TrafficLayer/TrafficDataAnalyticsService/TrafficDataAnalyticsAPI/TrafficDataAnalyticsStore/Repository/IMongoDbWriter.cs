using System;
using TrafficDataAnalyticsData.Collections;

namespace TrafficDataAnalyticsStore.Repository;

public interface IMongoDbWriter
{
    Task<List<string>> GetAllIntersectionIdsAsync();
    Task InsertDailySummaryAsync(DailySummary summary);
}
