using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using TrafficAnalyticsData.Settings;

namespace TrafficAnalyticsData.Healthchecks;

public class TrafficAnalyticsDbHealthCheck : IHealthCheck
{
    private readonly TrafficAnalyticsDbSettings _settings;

    public TrafficAnalyticsDbHealthCheck(TrafficAnalyticsDbSettings settings)
    {
        _settings = settings;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_settings.ConnectionString);
            await conn.OpenAsync(cancellationToken);
            return conn.State == System.Data.ConnectionState.Open
                ? HealthCheckResult.Healthy("TrafficAnalyticsDB reachable.")
                : HealthCheckResult.Unhealthy("TrafficAnalyticsDB unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Error: {ex.Message}");
        }
    }
}