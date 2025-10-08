using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserData;

public class UserDbHealthCheck : IHealthCheck
{
    private readonly UserDbContext _context;

    public UserDbHealthCheck(UserDbContext context)
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
                ? HealthCheckResult.Healthy("UserDB (MSSQL) reachable.")
                : HealthCheckResult.Unhealthy("UserDB (MSSQL) unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"UserDB health check failed: {ex.Message}");
        }
    }
}
