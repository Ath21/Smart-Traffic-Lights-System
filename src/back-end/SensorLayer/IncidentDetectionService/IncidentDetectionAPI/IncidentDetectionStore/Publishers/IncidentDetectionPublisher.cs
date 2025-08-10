using System;
using System.Threading.Tasks;
using MassTransit;
using SensorMessages.Data;
using SensorMessages.Logs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace IncidentDetectionStore.Publishers
{
    public class IncidentDetectionPublisher : IIncidentDetectionPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<IncidentDetectionPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _sensorDataExchange;
        private readonly string _incidentReportRoutingKeyBase;
        private readonly string _auditRoutingKey;
        private readonly string _errorRoutingKey;

        public IncidentDetectionPublisher(
            IBus bus,
            ILogger<IncidentDetectionPublisher> logger,
            IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _incidentReportRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorIncidentDetectionReportKey"] ?? "sensor.incident_detection.report.*";
            _auditRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsAuditKey"] ?? "sensor.incident_detection.logs.audit";
            _errorRoutingKey = _configuration["RabbitMQ:RoutingKey:SensorLogsErrorKey"] ?? "sensor.incident_detection.logs.error";
        }

        public async Task PublishIncidentReportAsync(
            Guid intersectionId,
            string incidentType,
            string severity,
            string description,
            DateTime timestamp
        )
        {
            var routingKeyBase = _incidentReportRoutingKeyBase.TrimEnd('*').TrimEnd('.');
            var routingKey = $"{routingKeyBase}.{intersectionId}";

            var message = new IncidentDetectionMessage(
                intersectionId,
                incidentType,
                severity,
                description,
                timestamp
            );

            _logger.LogInformation("[INCIDENT DATA] Publishing incident report to '{RoutingKey}' on exchange '{Exchange}'",
                routingKey, _sensorDataExchange);

            await _bus.Publish(message, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[INCIDENT DATA] Published incident report for intersection {IntersectionId}: {Type}, Severity {Severity}, {Description}",
                intersectionId, incidentType, severity, description);
        }

        public async Task PublishAuditLogAsync(string message)
        {
            _logger.LogInformation("[AUDIT] Publishing audit log to '{RoutingKey}'", _auditRoutingKey);

            var logMessage = new AuditLogMessage("IncidentDetectionService", message, DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_auditRoutingKey);
            });

            _logger.LogInformation("[AUDIT] Audit log published");
        }

        public async Task PublishErrorLogAsync(string message, Exception exception)
        {
            _logger.LogInformation("[ERROR] Publishing error log to '{RoutingKey}'", _errorRoutingKey);

            var logMessage = new ErrorLogMessage("IncidentDetectionService", message, exception.ToString(), DateTime.UtcNow);

            await _bus.Publish(logMessage, context =>
            {
                context.SetRoutingKey(_errorRoutingKey);
            });

            _logger.LogInformation("[ERROR] Error log published");
        }
    }
}
