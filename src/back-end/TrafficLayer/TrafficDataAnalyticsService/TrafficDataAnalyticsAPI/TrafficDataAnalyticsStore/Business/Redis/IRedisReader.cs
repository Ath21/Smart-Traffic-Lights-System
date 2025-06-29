using System;
using TrafficDataAnalyticsData.Collections;

namespace TrafficDataAnalyticsStore.Business.Redis;

public interface IRedisReader
{
    Task<DailySummary?> ComputeDailySummaryAsync(string intersectionId);
    Task<List<string>> GetVehicleKeysForIntersection(string id);
}
