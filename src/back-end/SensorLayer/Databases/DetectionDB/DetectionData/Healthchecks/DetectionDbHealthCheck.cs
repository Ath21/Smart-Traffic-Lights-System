using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DetectionData.Healthchecks;

public class DetectionDbHealthCheck : IHealthCheck
{
    private readonly DetectionDbContext _context;

    public DetectionDbHealthCheck(DetectionDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        bool canConnect = await _context.CanConnectAsync();
        return canConnect
            ? HealthCheckResult.Healthy("DetectionDB reachable.")
            : HealthCheckResult.Unhealthy("Cannot connect to DetectionDB.");
    }
}

