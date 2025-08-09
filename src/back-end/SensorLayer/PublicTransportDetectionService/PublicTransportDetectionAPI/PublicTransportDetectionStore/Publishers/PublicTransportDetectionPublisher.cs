using MassTransit;
using SensorMessages.Logs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using SensorMessages.Data;

namespace PublicTransportDetectionStore.Publishers
{
    public class PublicTransportDetectionPublisher : IPublicTransportDetectionPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<PublicTransportDetectionPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _sensorDataExchange;
        private readonly string _publicTransportDetectionRoutingKeyBase;
        private readonly string _auditRoutingKey;
        private readonly string _errorRoutingKey;

        public PublicTransportDetectionPublisher(IBus bus, ILogger<PublicTransportDetectionPublisher> logger, IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] 
                                  ?? "sensor.data.exchange";

            _publicTransportDetectionRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorPublicTransportDetectionDetectionKey"] 
                                  ?? "sensor.public_transport_detection.detection.*";

            _auditRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsAuditKey"] 
                                  ?? "sensor.public_transport_detection.logs.audit";

            _errorRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsErrorKey"] 
                                  ?? "sensor.public_transport_detection.logs.error";
        }

        public async Task PublishPublicTransportDetectionAsync(
            Guid intersectionId, 
            string routeId, 
            string vehicleType,    // Added vehicleType parameter
            int passengerCount,    // Added passengerCount parameter
            DateTime timestamp)
        {
            var routingKeyBase = _publicTransportDetectionRoutingKeyBase.TrimEnd('*').TrimEnd('.');
            var routingKey = $"{routingKeyBase}.{intersectionId}";

            var message = new PublicTransportDetectionMessage(
                intersectionId,
                routeId,
                vehicleType,
                passengerCount,
                timestamp
            );

            _logger.LogInformation("[PUBLIC TRANSPORT DATA] Publishing detection to '{RoutingKey}' on exchange '{Exchange}'",
                routingKey, _sensorDataExchange);

            await _bus.Publish(message, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[PUBLIC TRANSPORT DATA] Published detection for route {RouteId} (VehicleType: {VehicleType}) with {PassengerCount} passengers at intersection {IntersectionId}",
                routeId, vehicleType, passengerCount, intersectionId);
        }

        public async Task PublishAuditLogAsync(string message)
        {
            _logger.LogInformation("[AUDIT] Publishing audit log to '{RoutingKey}'", _auditRoutingKey);

            var logMessage = new AuditLogMessage("PublicTransportDetectionService", message, DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_auditRoutingKey);
            });

            _logger.LogInformation("[AUDIT] Audit log published");
        }

        public async Task PublishErrorLogAsync(string message, Exception exception)
        {
            _logger.LogInformation("[ERROR] Publishing error log to '{RoutingKey}'", _errorRoutingKey);

            var logMessage = new ErrorLogMessage("PublicTransportDetectionService", message, exception.ToString(), DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_errorRoutingKey);
            });

            _logger.LogInformation("[ERROR] Error log published");
        }
    }
}
