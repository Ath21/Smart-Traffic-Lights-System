using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DetectionCacheData.Healthchecks;

public class DetectionCacheDbHealthCheck : IHealthCheck
{
    private readonly DetectionCacheDbContext _context;

    public DetectionCacheDbHealthCheck(DetectionCacheDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        bool canConnect = await _context.CanConnectAsync();
        return canConnect
            ? HealthCheckResult.Healthy("DetectionCacheDB reachable.")
            : HealthCheckResult.Unhealthy("Cannot connect to DetectionCacheDB.");
    }
}
