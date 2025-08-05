using System;

namespace VehicleDetectionStore.Publishers;


public class VehicleDataPublisher : IVehicleDataPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<VehicleDataPublisher> _logger;

    public VehicleDataPublisher(IPublishEndpoint publishEndpoint, ILogger<VehicleDataPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishVehicleCountAsync(Guid intersectionId, int vehicleCount, double avgSpeed, DateTime timestamp)
    {
        var topic = $"sensor.data.vehicle.count.{intersectionId}";

        var message = new VehicleCountMessage
        {
            IntersectionId = intersectionId,
            VehicleCount = vehicleCount,
            AvgSpeed = avgSpeed,
            Timestamp = timestamp
        };

        _logger.LogInformation("Publishing vehicle count to {Topic}", topic);
        await _publishEndpoint.Publish(message, ctx => ctx.SetRoutingKey(topic));
    }

    public async Task PublishAuditLogAsync(string message)
    {
        var log = new AuditLogMessage
        {
            Service = "VehicleDetectionService",
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Publishing audit log: {Message}", message);
        await _publishEndpoint.Publish(log, ctx => ctx.SetRoutingKey("sensor.logs.audit"));
    }

    public async Task PublishErrorLogAsync(string message, Exception ex)
    {
        var log = new ErrorLogMessage
        {
            Service = "VehicleDetectionService",
            Message = message,
            Exception = ex.ToString(),
            Timestamp = DateTime.UtcNow
        };

        _logger.LogError(ex, "Publishing error log: {Message}", message);
        await _publishEndpoint.Publish(log, ctx => ctx.SetRoutingKey("sensor.logs.error"));
    }
}