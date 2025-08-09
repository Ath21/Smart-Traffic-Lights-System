using MassTransit;
using SensorMessages.Logs;
using SensorMessages.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using EmergencyVehicleDetectionStore.Publishers;

namespace EmergencyVehicleDetectionService.Publishers
{
    public class EmergencyVehicleDetectionPublisher : IEmergencyVehicleDetectionPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<EmergencyVehicleDetectionPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _sensorDataExchange;
        private readonly string _emergencyDetectionRoutingKeyBase;
        private readonly string _auditRoutingKey;
        private readonly string _errorRoutingKey;

        public EmergencyVehicleDetectionPublisher(IBus bus, ILogger<EmergencyVehicleDetectionPublisher> logger, IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _emergencyDetectionRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorEmergencyVehicleDetectionDetectionKey"] ?? "sensor.emergency_vehicle_detection.detection.*";
            _auditRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsAuditKey"] ?? "sensor.emergency_vehicle_detection.logs.audit";
            _errorRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsErrorKey"] ?? "sensor.emergency_vehicle_detection.logs.error";
        }

        public async Task PublishEmergencyVehicleDetectionAsync(Guid intersectionId, string vehicleId, string vehicleType, double speed, DateTime timestamp)
        {
            // Remove trailing '*' and '.' from routing key base, then append intersectionId
            var routingKeyBase = _emergencyDetectionRoutingKeyBase.TrimEnd('*').TrimEnd('.');
            var routingKey = $"{routingKeyBase}.{intersectionId}";

            var message = new EmergencyVehicleDetectionMessage(intersectionId, vehicleId, vehicleType, speed, timestamp);

            _logger.LogInformation("[EMERGENCY VEHICLE DATA] Publishing detection to '{RoutingKey}' on exchange '{Exchange}'", routingKey, _sensorDataExchange);

            await _bus.Publish(message, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[EMERGENCY VEHICLE DATA] Published detection for vehicle {VehicleId} ({VehicleType}) at speed {Speed} km/h", vehicleId, vehicleType, speed);
        }

        public async Task PublishAuditLogAsync(string message)
        {
            _logger.LogInformation("[AUDIT] Publishing audit log to '{RoutingKey}'", _auditRoutingKey);

            var logMessage = new AuditLogMessage("EmergencyVehicleDetectionService", message, DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_auditRoutingKey);
            });

            _logger.LogInformation("[AUDIT] Audit log published");
        }

        public async Task PublishErrorLogAsync(string message, Exception exception)
        {
            _logger.LogInformation("[ERROR] Publishing error log to '{RoutingKey}'", _errorRoutingKey);

            var logMessage = new ErrorLogMessage("EmergencyVehicleDetectionService", message, exception.ToString(), DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_errorRoutingKey);
            });

            _logger.LogInformation("[ERROR] Error log published");
        }
    }
}
