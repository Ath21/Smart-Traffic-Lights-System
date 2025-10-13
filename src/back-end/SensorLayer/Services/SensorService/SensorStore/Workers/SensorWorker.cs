using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensorStore.Domain;
using SensorStore.Business;
using SensorStore.Publishers.Count;
using SensorStore.Publishers.Logs;
using Messages.Sensor;
using Messages.Log;

namespace SensorStore.Workers;

// ============================================================
// Sensor Worker (Fog Layer)
// ------------------------------------------------------------
// Simulates real-time traffic flow measurements for a given
// intersection. Publishes count data (vehicle, pedestrian,
// cyclist) via RabbitMQ and persists them via SensorBusiness.
// ------------------------------------------------------------
// Publishes: sensor.count.{intersection}.{type}
// Logs:      log.sensor.sensor-service.{type}
// ============================================================

public class SensorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorWorker> _logger;
    private readonly IntersectionContext _intersection;
    private readonly Random _rand = new();

    public SensorWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<SensorWorker> logger,
        IntersectionContext intersection)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intersection = intersection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[WORKER][SENSOR] Started for {IntersectionName} (Id={IntersectionId})",
            _intersection.Name, _intersection.Id);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var business = scope.ServiceProvider.GetRequiredService<ISensorBusiness>();
                var countPublisher = scope.ServiceProvider.GetRequiredService<ISensorCountPublisher>();
                var logPublisher = scope.ServiceProvider.GetRequiredService<ISensorLogPublisher>();

                // ============================================================
                // VEHICLE COUNT
                // ============================================================
                var vehicleCount = _rand.Next(10, 120);
                var avgSpeed = Math.Round(_rand.NextDouble() * 40 + 20, 1);
                var avgWait = Math.Round(_rand.NextDouble() * 20, 1);
                var flowRate = Math.Round(vehicleCount / (avgWait + 1), 2);
                var breakdown = new Dictionary<string, int>
                {
                    ["car"] = _rand.Next(10, 80),
                    ["bus"] = _rand.Next(0, 10),
                    ["truck"] = _rand.Next(0, 8),
                    ["motorcycle"] = _rand.Next(0, 15)
                };

                var vehicleCorrelationId = Guid.NewGuid();

                await countPublisher.PublishVehicleCountAsync(
                    vehicleCount, avgSpeed, avgWait, flowRate, breakdown, vehicleCorrelationId);

                await logPublisher.PublishAuditAsync(
                    "VehicleCountPublished",
                    $"Vehicle count ({vehicleCount}) published for {_intersection.Name}",
                    null,
                    vehicleCorrelationId);

                await business.ProcessSensorAsync(new SensorCountMessage
                {
                    CorrelationId = vehicleCorrelationId,
                    Timestamp = DateTime.UtcNow,
                    SourceLayer = "Sensor Layer",
                    DestinationLayer = new() { "Traffic Layer" },
                    SourceService = "Sensor Service",
                    DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },
                    IntersectionId = _intersection.Id,
                    IntersectionName = _intersection.Name,
                    CountType = "Vehicle",
                    Count = vehicleCount,
                    AverageSpeedKmh = avgSpeed,
                    AverageWaitTimeSec = avgWait,
                    FlowRate = flowRate,
                    Breakdown = breakdown
                });

                // ============================================================
                // PEDESTRIAN COUNT
                // ============================================================
                var pedCount = _rand.Next(5, 80);
                var pedCorrelationId = Guid.NewGuid();

                await countPublisher.PublishPedestrianCountAsync(pedCount, pedCorrelationId);

                await logPublisher.PublishAuditAsync(
                    "PedestrianCountPublished",
                    $"Pedestrian count ({pedCount}) published for {_intersection.Name}",
                    null,
                    pedCorrelationId);

                await business.ProcessSensorAsync(new SensorCountMessage
                {
                    CorrelationId = pedCorrelationId,
                    Timestamp = DateTime.UtcNow,
                    SourceLayer = "Sensor Layer",
                    DestinationLayer = new() { "Traffic Layer" },
                    SourceService = "Sensor Service",
                    DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },
                    IntersectionId = _intersection.Id,
                    IntersectionName = _intersection.Name,
                    CountType = "Pedestrian",
                    Count = pedCount,
                    AverageSpeedKmh = 0,
                    AverageWaitTimeSec = 0,
                    FlowRate = 0
                });

                // ============================================================
                // CYCLIST COUNT
                // ============================================================
                var cyclistCount = _rand.Next(0, 20);
                var cycCorrelationId = Guid.NewGuid();

                await countPublisher.PublishCyclistCountAsync(cyclistCount, cycCorrelationId);

                await logPublisher.PublishAuditAsync(
                    "CyclistCountPublished",
                    $"Cyclist count ({cyclistCount}) published for {_intersection.Name}",
                    null,
                    cycCorrelationId);

                await business.ProcessSensorAsync(new SensorCountMessage
                {
                    CorrelationId = cycCorrelationId,
                    Timestamp = DateTime.UtcNow,
                    SourceLayer = "Sensor Layer",
                    DestinationLayer = new() { "Traffic Layer" },
                    SourceService = "Sensor Service",
                    DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },
                    IntersectionId = _intersection.Id,
                    IntersectionName = _intersection.Name,
                    CountType = "Cyclist",
                    Count = cyclistCount,
                    AverageSpeedKmh = 0,
                    AverageWaitTimeSec = 0,
                    FlowRate = Math.Round(cyclistCount / 30.0, 2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[WORKER][SENSOR] Error in SensorWorker loop for {IntersectionName}",
                    _intersection.Name);
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }

        _logger.LogInformation("[WORKER][SENSOR] Stopped for {IntersectionName}", _intersection.Name);
    }
}
