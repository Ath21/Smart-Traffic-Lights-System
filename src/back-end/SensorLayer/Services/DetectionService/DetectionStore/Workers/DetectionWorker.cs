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
                        var vehicleType = _rand.NextDouble() < 0.5 ? "ambulance" : "firetruck";

                        var metadata = new Dictionary<string, string>
                        {
                            ["intersection_id"] = _intersection.Id.ToString(),
                            ["intersection_name"] = _intersection.Name,
                            ["direction"] = direction,
                            ["vehicle_type"] = vehicleType
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
                            ["intersection_id"] = _intersection.Id.ToString(),
                            ["intersection_name"] = _intersection.Name,
                            ["line_name"] = line,
                            ["direction"] = direction
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
                            ["intersection_id"] = _intersection.Id.ToString(),
                            ["intersection_name"] = _intersection.Name,
                            ["description"] = "Random simulated traffic incident",
                            ["direction"] = direction
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

        private static string GetRandomDirection()
        {
            var directions = new[] { "north", "south", "east", "west" };
            return directions[Random.Shared.Next(directions.Length)];
        }
    }
}
