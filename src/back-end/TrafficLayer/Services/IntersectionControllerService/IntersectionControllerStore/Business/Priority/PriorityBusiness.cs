using Messages.Sensor;
using Messages.Traffic;
using Microsoft.Extensions.Logging;
using IntersectionControllerStore.Publishers;
using IntersectionControllerStore.Publishers.Priority;
using IntersectionControllerStore.Publishers.Logs;

namespace IntersectionControllerStore.Business.Priority;

public class PriorityBusiness : IPriorityBusiness
{
    private readonly IPriorityPublisher _priorityPublisher;
    private readonly IIntersectionLogPublisher _logPublisher;
    private readonly ILogger<PriorityBusiness> _logger;

    public PriorityBusiness(
        IPriorityPublisher priorityPublisher,
        IIntersectionLogPublisher logPublisher,
        ILogger<PriorityBusiness> logger)
    {
        _priorityPublisher = priorityPublisher;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task HandleDetectionEventAsync(DetectionEventMessage msg)
    {
        try
        {
            int level = msg.EventType?.ToLowerInvariant() switch
            {
                "emergency vehicle" => 3,
                "public transport" => 2,
                "incident" => 1,
                _ => 1
            };

            var priorityMsg = new PriorityEventMessage
            {
                CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId,
                Timestamp = DateTime.UtcNow,
                SourceLayer = "Control Layer",
                DestinationLayer = new() { "Traffic Layer" },
                SourceService = "Intersection Controller Service",
                DestinationServices = new() { "Traffic Light Coordinator Service" },
                IntersectionId = msg.IntersectionId,
                IntersectionName = msg.IntersectionName,
                EventType = msg.EventType,
                VehicleType = msg.VehicleType,
                Direction = msg.Direction,
                PriorityLevel = level
            };

            await _priorityPublisher.PublishPriorityEventAsync(priorityMsg);

            await _logPublisher.PublishAuditAsync(
                "PriorityEventPublished",
                $"Priority {level} event published for {msg.IntersectionName}",
                new()
                {
                    ["event_type"] = msg.EventType ?? "",
                    ["vehicle_type"] = msg.VehicleType ?? "",
                    ["priority_level"] = level.ToString()
                },
                msg.CorrelationId);
        }
        catch (Exception ex)
        {
            await _logPublisher.PublishErrorAsync(
                "PriorityEventError",
                $"Error handling detection event at {msg.IntersectionName}",
                ex,
                new() { ["event_type"] = msg.EventType ?? "unknown" },
                msg.CorrelationId);
            throw;
        }
    }

    public async Task HandleSensorCountAsync(SensorCountMessage msg)
    {
        try
        {
            var threshold = msg.CountType?.ToLowerInvariant() switch
            {
                "vehicle" => 120,
                "pedestrian" => 50,
                "cyclist" => 25,
                _ => 80
            };

            bool exceeded = msg.Count > threshold;
            int level = exceeded ? 2 : 1;

            var priorityMsg = new PriorityCountMessage
            {
                CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId,
                Timestamp = DateTime.UtcNow,
                SourceLayer = "Control Layer",
                DestinationLayer = new() { "Traffic Layer" },
                SourceService = "Intersection Controller Service",
                DestinationServices = new() { "Traffic Light Coordinator Service" },
                IntersectionId = msg.IntersectionId,
                IntersectionName = msg.IntersectionName,
                CountType = msg.CountType,
                TotalCount = msg.Count,
                PriorityLevel = level,
                IsThresholdExceeded = exceeded
            };

            await _priorityPublisher.PublishPriorityCountAsync(priorityMsg);

            await _logPublisher.PublishAuditAsync(
                "PriorityCountPublished",
                $"Priority {level} count published for {msg.IntersectionName}",
                new()
                {
                    ["count_type"] = msg.CountType ?? "",
                    ["total_count"] = msg.Count.ToString(),
                    ["priority_level"] = level.ToString()
                },
                msg.CorrelationId);
        }
        catch (Exception ex)
        {
            await _logPublisher.PublishErrorAsync(
                "PriorityCountError",
                $"Error handling count for {msg.IntersectionName}",
                ex,
                new() { ["count_type"] = msg.CountType ?? "unknown" },
                msg.CorrelationId);
            throw;
        }
    }
}
