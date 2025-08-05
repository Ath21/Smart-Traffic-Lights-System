using MassTransit;
using SensorMessages.Logs;
using SensorMessages.Data;
using VehicleDetectionStore.Publishers;

namespace VehicleDetectionService.Publishers;

public class VehicleDetectionPublisher : IVehicleDetectionPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<VehicleDetectionPublisher> _logger;

    private readonly string _auditKey = "sensor.logs.audit";
    private readonly string _errorKey = "sensor.logs.error";

    public VehicleDetectionPublisher(IBus bus, ILogger<VehicleDetectionPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishVehicleCountAsync(Guid intersectionId, int vehicleCount, double avgSpeed, DateTime timestamp)
    {
        var routingKey = $"sensor.data.vehicle.count.{intersectionId}";
        var message = new VehicleCountMessage(intersectionId, vehicleCount, avgSpeed, timestamp);

        _logger.LogInformation("[VEHICLE DATA] Publishing vehicle count to '{RoutingKey}'", routingKey);

        await _bus.Publish(message, context =>
        {
            context.SetRoutingKey(routingKey);
        });

        _logger.LogInformation("[VEHICLE DATA] Published: {Count} vehicles, {Speed} km/h avg", vehicleCount, avgSpeed);
    }

    public async Task PublishAuditLogAsync(string message)
    {
        _logger.LogInformation("[AUDIT] Publishing audit log to '{RoutingKey}'", _auditKey);

        var logMessage = new AuditLogMessage("VehicleDetectionService", message, DateTime.UtcNow);

        await _bus.Publish(logMessage, context =>
        {
            context.SetRoutingKey(_auditKey);
        });

        _logger.LogInformation("[AUDIT] Audit log published");
    }

    public async Task PublishErrorLogAsync(string message, Exception exception)
    {
        _logger.LogInformation("[ERROR] Publishing error log to '{RoutingKey}'", _errorKey);

        var logMessage = new ErrorLogMessage("VehicleDetectionService", message, exception.ToString(), DateTime.UtcNow);

        await _bus.Publish(logMessage, context =>
        {
            context.SetRoutingKey(_errorKey);
        });

        _logger.LogInformation("[ERROR] Error log published");
    }
}
