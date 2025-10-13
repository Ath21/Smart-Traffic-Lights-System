using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DetectionStore.Domain;
using DetectionStore.Business;
using DetectionStore.Publishers.Event;
using DetectionStore.Publishers.Logs;
using Messages.Log;
using Messages.Sensor;

namespace DetectionStore.Workers
{
    public class DetectionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DetectionWorker> _logger;
        private readonly IntersectionContext _intersection;
        private readonly Random _rand = new();

        public DetectionWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<DetectionWorker> logger,
            IntersectionContext intersection)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _intersection = intersection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "[WORKER][DETECTION] Started for {IntersectionName} (Id={IntersectionId})",
                _intersection.Name, _intersection.Id);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IDetectionEventPublisher>();
                    var logPublisher   = scope.ServiceProvider.GetRequiredService<IDetectionLogPublisher>();
                    var business       = scope.ServiceProvider.GetRequiredService<IDetectionBusiness>();

                    // ============================================================
                    // EMERGENCY VEHICLE DETECTION (~10%)
                    // ============================================================
                    if (_rand.NextDouble() < 0.1)
                    {
                        var direction = GetRandomDirection();
                        var vehicleType = _rand.Next(3) switch
                        {
                            0 => "ambulance",
                            1 => "firetruck",
                            _ => "police car"
                        };

                        var metadata = new Dictionary<string, string>
                        {
                            ["speed_kmh"] = _rand.Next(30, 110).ToString(),
                            ["signal_priority_level"] = vehicleType switch
                            {
                                "ambulance" => "high",
                                "firetruck" => "medium",
                                "police car" => "law-enforcement",
                                _ => "normal"
                            },
                            ["last_known_location"] = GetRandomCoordinates(),
                            ["sirens_detected"] = (_rand.NextDouble() > 0.2).ToString().ToLower(),
                            ["approach_estimate_seconds"] = _rand.Next(3, 10).ToString(),
                        };

                        var correlationId = Guid.NewGuid();

                        var detectionMsg = await eventPublisher.PublishEmergencyVehicleAsync(vehicleType, direction, correlationId, metadata);

                        var logMsg = await logPublisher.PublishAuditAsync(
                            "EmergencyVehicleDetected",
                            $"Emergency vehicle ({vehicleType}) detected at {_intersection.Name}",
                            metadata,
                            correlationId);

                        await business.ProcessDetectionAsync(detectionMsg);
                    }

                    // ============================================================
                    // PUBLIC TRANSPORT (~30%)
                    // ============================================================
                    if (_rand.NextDouble() < 0.3)
                    {
                        var line = $"Bus{_rand.Next(700, 899)}";
                        var direction = GetRandomDirection();

                        var metadata = new Dictionary<string, string>
                        {
                            ["speed_kmh"] = _rand.Next(20, 70).ToString(),
                            ["occupancy_ratio"] = _rand.NextDouble().ToString("0.00"),
                            ["schedule_deviation_min"] = _rand.Next(-3, 5).ToString(), // early(-)/late(+)
                            ["priority_reason"] = _rand.NextDouble() < 0.5 ? "running_late" : "schedule_priority"
                        };


                        var correlationId = Guid.NewGuid();

                        var detectionMsg = await eventPublisher.PublishPublicTransportAsync(line, direction, correlationId, metadata);
                        var logMsg = await logPublisher.PublishAuditAsync(
                            "PublicTransportDetected",
                            $"Public transport {line} detected at {_intersection.Name}",
                            metadata,
                            correlationId);

                        await business.ProcessDetectionAsync(detectionMsg);
                    }

                    // ============================================================
                    // INCIDENT (~5%)
                    // ============================================================
                    if (_rand.NextDouble() < 0.05)
                    {
                        var direction = GetRandomDirection();

                        var metadata = new Dictionary<string, string>
                        {
                            ["incident_type"] = GetRandomIncidentType(),
                            ["severity_level"] = _rand.Next(1, 5).ToString(),
                            ["lanes_blocked"] = _rand.Next(0, 2).ToString(),
                            ["estimated_duration_min"] = _rand.Next(5, 30).ToString()
                        };

                        var correlationId = Guid.NewGuid();

                        var detectionMsg = await eventPublisher.PublishIncidentAsync("unknown", direction, correlationId, metadata);
                        var logMsg = await logPublisher.PublishFailoverAsync(
                            "IncidentReported",
                            $"Incident reported at {_intersection.Name}",
                            metadata,
                            correlationId);

                        await business.ProcessDetectionAsync(detectionMsg);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[WORKER][DETECTION] Error in DetectionWorker loop for {IntersectionName}",
                        _intersection.Name);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("[WORKER][DETECTION] Stopped for {IntersectionName}", _intersection.Name);
        }


    // ============================================================
    // Helper utilities
    // ============================================================
    private static string GetRandomDirection()
    {
        var directions = new[] { "north", "south", "east", "west" };
        return directions[Random.Shared.Next(directions.Length)];
    }

    private static string GetRandomEmergencyType()
    {
        var types = new[] { "ambulance", "firetruck", "police car" };
        return types[Random.Shared.Next(types.Length)];
    }

    private static string GetRandomIncidentType()
    {
        var types = new[] { "collision", "breakdown", "roadwork", "obstruction" };
        return types[Random.Shared.Next(types.Length)];
    }

    private string GetRandomCoordinates()
    {
        // random offset near intersection (example Athens area)
        var baseLat = 37.9750;
        var baseLon = 23.7350;
        var lat = baseLat + (_rand.NextDouble() - 0.5) * 0.002; // ±0.001 deg ~ ±100m
        var lon = baseLon + (_rand.NextDouble() - 0.5) * 0.002;
        return $"{lat:F6}, {lon:F6}";
    }
    }
}
