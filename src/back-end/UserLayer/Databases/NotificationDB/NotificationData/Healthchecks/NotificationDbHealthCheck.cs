using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotificationData;

public class NotificationDbHealthCheck : IHealthCheck
{
    private readonly NotificationDbContext _context;

    public NotificationDbHealthCheck(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ok = await _context.CanConnectAsync();
            return ok
                ? HealthCheckResult.Healthy("NotificationDB (Mongo) reachable.")
                : HealthCheckResult.Unhealthy("NotificationDB (Mongo) unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"NotificationDB check failed: {ex.Message}");
        }
    }
}
