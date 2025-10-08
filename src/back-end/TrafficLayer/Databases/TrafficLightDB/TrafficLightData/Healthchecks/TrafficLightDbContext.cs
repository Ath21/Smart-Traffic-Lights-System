using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TrafficLightData.Settings;

namespace TrafficLightData.Healthchecks;

public class TrafficLightDbHealthCheck : IHealthCheck
{
    private readonly TrafficLightDbSettings _settings;

    public TrafficLightDbHealthCheck(TrafficLightDbSettings settings)
    {
        _settings = settings;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_settings.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            if (connection.State == System.Data.ConnectionState.Open)
                return HealthCheckResult.Healthy("TrafficLightDB reachable.");

            return HealthCheckResult.Unhealthy("TrafficLightDB unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"MSSQL connection error: {ex.Message}");
        }
    }
}
