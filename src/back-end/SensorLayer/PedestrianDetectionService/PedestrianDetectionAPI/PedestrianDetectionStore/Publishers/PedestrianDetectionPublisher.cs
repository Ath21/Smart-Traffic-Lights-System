using System;
using MassTransit;
using SensorMessages.Data;
using SensorMessages.Logs;

namespace PedestrianDetectionStore.Publishers;

public class PedestrianDetectionPublisher : IPedestrianDetectionPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<PedestrianDetectionPublisher> _logger;
    private readonly IConfiguration _configuration;

    private readonly string _sensorDataExchange;
    private readonly string _pedestrianRequestRoutingKeyBase;
    private readonly string _auditRoutingKey;
    private readonly string _errorRoutingKey;

    public PedestrianDetectionPublisher(
        IBus bus,
        ILogger<PedestrianDetectionPublisher> logger,
        IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;
        _configuration = configuration;

        _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
        _pedestrianRequestRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorPedestrianDetectionRequestKey"] ?? "sensor.pedestrian_detection.request.*";
        _auditRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsAuditKey"] ?? "sensor.pedestrian_detection.logs.audit";
        _errorRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsErrorKey"] ?? "sensor.pedestrian_detection.logs.error";
    }

    public async Task PublishPedestrianRequestAsync(Guid intersectionId, int count, DateTime timestamp)
    {
        var routingKeyBase = _pedestrianRequestRoutingKeyBase.TrimEnd('*').TrimEnd('.');
        var routingKey = $"{routingKeyBase}.{intersectionId}";

        var message = new PedestrianDetectionMessage(intersectionId, count, timestamp);

        _logger.LogInformation("[PEDESTRIAN DATA] Publishing pedestrian detection request to '{RoutingKey}' on exchange '{Exchange}'", routingKey, _sensorDataExchange);

        await _bus.Publish(message, context =>
        {
            context.SetRoutingKey(routingKey);
        });

        _logger.LogInformation("[PEDESTRIAN DATA] Published: {Count} pedestrians detected", count);
    }

    public async Task PublishAuditLogAsync(string message)
    {
        _logger.LogInformation("[AUDIT] Publishing audit log to '{RoutingKey}'", _auditRoutingKey);

        var logMessage = new AuditLogMessage("PedestrianDetectionService", message, DateTime.UtcNow);

        await _bus.Publish(logMessage, context =>
        {
            context.SetRoutingKey(_auditRoutingKey);
        });

        _logger.LogInformation("[AUDIT] Audit log published");
    }

    public async Task PublishErrorLogAsync(string message, Exception exception)
    {
        _logger.LogInformation("[ERROR] Publishing error log to '{RoutingKey}'", _errorRoutingKey);

        var logMessage = new ErrorLogMessage("PedestrianDetectionService", message, exception.ToString(), DateTime.UtcNow);

        await _bus.Publish(logMessage, context =>
        {
            context.SetRoutingKey(_errorRoutingKey);
        });

        _logger.LogInformation("[ERROR] Error log published");
    }
}
