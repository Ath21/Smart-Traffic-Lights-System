using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Sensor;
using SensorStore.Domain;
using SensorStore.Publishers.Count;
using System.Reflection.Metadata.Ecma335;

namespace SensorStore.Publishers.Count;

public class SensorCountPublisher : ISensorCountPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorCountPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public SensorCountPublisher(
        IConfiguration config,
        ILogger<SensorCountPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Sensor:Count"]
                          ?? "sensor.count.{intersection}.{type}";
    }

    // ============================================================
    // VEHICLE COUNT
    // ============================================================
    public async Task PublishVehicleCountAsync(
        int count,
        double avgSpeed,
        double avgWait,
        double flowRate,
        Dictionary<string, int>? breakdown = null,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new SensorCountMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new() { "Traffic Layer" },

            SourceService = "Sensor Service",
            DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },

            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,

            CountType = "Vehicle",
            Count = count,
            AverageSpeedKmh = avgSpeed,
            AverageWaitTimeSec = avgWait,
            FlowRate = flowRate,
            Breakdown = breakdown,

            Metadata = metadata
        };

        await PublishAsync("vehicle", msg);

        _logger.LogInformation(
            "[PUBLISHER][COUNT][{Intersection}] VEHICLE count published: {Count} (Speed={Speed:F1} km/h, Wait={Wait:F1}s, Flow={Flow:F2}/s)",
            _intersection.Name, count, avgSpeed, avgWait, flowRate);
    }

    // ============================================================
    // PEDESTRIAN COUNT
    // ============================================================
    public async Task PublishPedestrianCountAsync(
        int count,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new SensorCountMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new() { "Traffic Layer" },

            SourceService = "Sensor Service",
            DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },

            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,

            CountType = "Pedestrian",
            Count = count,
            AverageSpeedKmh = 0,
            AverageWaitTimeSec = 0,
            FlowRate = 0,
            Breakdown = null,

            Metadata = metadata
        };

        await PublishAsync("pedestrian", msg);

        _logger.LogInformation(
            "[PUBLISHER][COUNT][{Intersection}] PEDESTRIAN count published: {Count}", _intersection.Name, count);
    }

    // ============================================================
    // CYCLIST COUNT
    // ============================================================
    public async Task PublishCyclistCountAsync(
        int count,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new SensorCountMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new() { "Traffic Layer" },

            SourceService = "Sensor Service",
            DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },

            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,

            CountType = "Cyclist",
            Count = count,
            AverageSpeedKmh = 0,
            AverageWaitTimeSec = 0,
            FlowRate = 0,
            Breakdown = null,

            Metadata = metadata
        };

        await PublishAsync("cyclist", msg);

        _logger.LogInformation(
            "[PUBLISHER][COUNT][{Intersection}] CYCLIST count published: {Count}", _intersection.Name, count);
    }

    // ============================================================
    // Internal publish logic
    // ============================================================
    private async Task PublishAsync(string typeKey, SensorCountMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{type}", typeKey);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
