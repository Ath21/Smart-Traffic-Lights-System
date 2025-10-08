using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogData.Healthchecks;

public class LogDbHealthCheck : IHealthCheck
{
    private readonly LogDbContext _context;

    public LogDbHealthCheck(LogDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        bool canConnect = await _context.CanConnectAsync();
        return canConnect
            ? HealthCheckResult.Healthy("LogDB reachable.")
            : HealthCheckResult.Unhealthy("Cannot connect to LogDB.");
    }
}
