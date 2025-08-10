using System;
using MassTransit;
using SensorMessages.Data;
using SensorMessages.Logs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CyclistDetectionStore.Publishers
{
    public class CyclistDetectionPublisher : ICyclistDetectionPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<CyclistDetectionPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _sensorDataExchange;
        private readonly string _cyclistRequestRoutingKeyBase;
        private readonly string _auditRoutingKey;
        private readonly string _errorRoutingKey;

        public CyclistDetectionPublisher(
            IBus bus,
            ILogger<CyclistDetectionPublisher> logger,
            IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _cyclistRequestRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorCyclistDetectionRequestKey"] ?? "sensor.cyclist_detection.request.*";
            _auditRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsAuditKey"] ?? "sensor.cyclist_detection.logs.audit";
            _errorRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsErrorKey"] ?? "sensor.cyclist_detection.logs.error";
        }

        public async Task PublishCyclistRequestAsync(Guid intersectionId, int cyclistCount, double avgSpeed, DateTime timestamp)
        {
            var routingKeyBase = _cyclistRequestRoutingKeyBase.TrimEnd('*').TrimEnd('.');
            var routingKey = $"{routingKeyBase}.{intersectionId}";

            var message = new CyclistDetectionMessage(intersectionId, cyclistCount, avgSpeed, timestamp);

            _logger.LogInformation("[CYCLIST DATA] Publishing cyclist detection request to '{RoutingKey}' on exchange '{Exchange}'", routingKey, _sensorDataExchange);

            await _bus.Publish(message, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[CYCLIST DATA] Published: {Count} cyclists detected at avg speed {Speed}", cyclistCount, avgSpeed);
        }

        public async Task PublishAuditLogAsync(string message)
        {
            _logger.LogInformation("[AUDIT] Publishing audit log to '{RoutingKey}'", _auditRoutingKey);

            var logMessage = new AuditLogMessage("CyclistDetectionService", message, DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_auditRoutingKey);
            });

            _logger.LogInformation("[AUDIT] Audit log published");
        }

        public async Task PublishErrorLogAsync(string message, Exception exception)
        {
            _logger.LogInformation("[ERROR] Publishing error log to '{RoutingKey}'", _errorRoutingKey);

            var logMessage = new ErrorLogMessage("CyclistDetectionService", message, exception.ToString(), DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_errorRoutingKey);
            });

            _logger.LogInformation("[ERROR] Error log published");
        }
    }
}
