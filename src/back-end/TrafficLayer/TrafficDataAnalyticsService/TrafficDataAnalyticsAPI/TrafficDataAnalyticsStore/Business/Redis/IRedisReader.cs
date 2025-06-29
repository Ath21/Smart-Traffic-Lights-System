using System;
using TrafficDataAnalyticsData.Collections;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.Redis;

public interface IRedisReader
{
    Task<DailySummaryDto?> ComputeDailySummaryAsync(string intersectionId);
    Task<List<string>> GetVehicleKeysForIntersection(string id);
}
