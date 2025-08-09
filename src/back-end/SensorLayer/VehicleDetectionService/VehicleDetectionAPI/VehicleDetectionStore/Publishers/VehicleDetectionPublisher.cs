using MassTransit;
using SensorMessages.Logs;
using SensorMessages.Data;
using System;

using VehicleDetectionStore.Publishers;

namespace VehicleDetectionService.Publishers
{
    public class VehicleDetectionPublisher : IVehicleDetectionPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<VehicleDetectionPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _sensorDataExchange;
        private readonly string _vehicleCountRoutingKeyBase;
        private readonly string _auditRoutingKey;
        private readonly string _errorRoutingKey;

        public VehicleDetectionPublisher(IBus bus, ILogger<VehicleDetectionPublisher> logger, IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            // Use your configured names or fallbacks
            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _vehicleCountRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorVehicleDetectionVehicleCountKey"] ?? "sensor.vehicle_detection.vehicle_count.*";
            _auditRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsAuditKey"] ?? "sensor.vehicle_detection.logs.audit";
            _errorRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsErrorKey"] ?? "sensor.vehicle_detection.logs.error";
        }

        public async Task PublishVehicleCountAsync(Guid intersectionId, int vehicleCount, double avgSpeed, DateTime timestamp)
        {
            // Remove trailing * if any from routing key base and append intersectionId
            var routingKeyBase = _vehicleCountRoutingKeyBase.TrimEnd('*').TrimEnd('.');
            var routingKey = $"{routingKeyBase}.{intersectionId}";

            var message = new VehicleCountMessage(intersectionId, vehicleCount, avgSpeed, timestamp);

            _logger.LogInformation("[VEHICLE DATA] Publishing vehicle count to '{RoutingKey}' on exchange '{Exchange}'", routingKey, _sensorDataExchange);

            await _bus.Publish(message, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[VEHICLE DATA] Published: {Count} vehicles, {Speed} km/h avg", vehicleCount, avgSpeed);
        }

        public async Task PublishAuditLogAsync(string message)
        {
            _logger.LogInformation("[AUDIT] Publishing audit log to '{RoutingKey}'", _auditRoutingKey);

            var logMessage = new AuditLogMessage("VehicleDetectionService", message, DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_auditRoutingKey);
            });

            _logger.LogInformation("[AUDIT] Audit log published");
        }

        public async Task PublishErrorLogAsync(string message, Exception exception)
        {
            _logger.LogInformation("[ERROR] Publishing error log to '{RoutingKey}'", _errorRoutingKey);

            var logMessage = new ErrorLogMessage("VehicleDetectionService", message, exception.ToString(), DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_errorRoutingKey);
            });

            _logger.LogInformation("[ERROR] Error log published");
        }
    }
}
