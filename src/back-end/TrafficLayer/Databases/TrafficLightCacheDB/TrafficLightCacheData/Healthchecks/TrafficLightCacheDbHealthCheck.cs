using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TrafficLightCacheData.Healthchecks;

public class TrafficLightCacheDbHealthCheck : IHealthCheck
{
    private readonly TrafficLightCacheDbContext _context;

    public TrafficLightCacheDbHealthCheck(TrafficLightCacheDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.CanConnectAsync();
            return canConnect
                ? HealthCheckResult.Healthy("TrafficLightCacheDB (Redis) reachable.")
                : HealthCheckResult.Unhealthy("TrafficLightCacheDB (Redis) unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis connection failed: {ex.Message}");
        }
    }
}
