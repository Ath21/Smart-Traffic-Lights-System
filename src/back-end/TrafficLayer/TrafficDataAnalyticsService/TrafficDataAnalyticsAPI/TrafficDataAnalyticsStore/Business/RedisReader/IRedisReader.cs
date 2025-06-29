using System;
using TrafficDataAnalyticsData.Collections;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.RedisReader;

public interface IRedisReader
{
    Task<DailySummaryDto?> ComputeDailySummaryAsync(string intersectionId);
    Task<List<string>> GetVehicleKeysForIntersection(string id);
}
