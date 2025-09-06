using System;

namespace SensorStore.Repositories.Metrics;

public interface IMetricRepository
{
    Task<int> GetMetricAsync(Guid intersectionId, string metricName);
    Task SetMetricAsync(Guid intersectionId, string metricName, int value);
    Task<bool> ExistsAsync(Guid intersectionId, string metricName);
}
