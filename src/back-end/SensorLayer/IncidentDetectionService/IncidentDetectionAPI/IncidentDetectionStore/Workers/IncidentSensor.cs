using IncidentDetectionStore.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SensorMessages.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IncidentDetectionStore.Workers
{
    public class IncidentSensor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IncidentSensor> _logger;
        private readonly IConfiguration _configuration;
        private readonly Guid _intersectionId = Guid.NewGuid();

        private readonly string _sensorDataExchange;
        private readonly string _incidentDetectionRoutingKeyBase;

        public IncidentSensor(IServiceProvider serviceProvider, ILogger<IncidentSensor> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _incidentDetectionRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorIncidentDetectionReportKey"] ?? "sensor.incident_detection.report.";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            string[] incidentTypes = { "collision", "obstruction", "roadwork", "breakdown" };
            string[] severities = { "low", "medium", "high" };

            while (!stoppingToken.IsCancellationRequested)
            {
                // Simulate random incident data
                var incidentType = incidentTypes[random.Next(incidentTypes.Length)];
                var severity = severities[random.Next(severities.Length)];
                var description = $"Simulated {incidentType} incident with {severity} severity.";
                var timestamp = DateTime.UtcNow;

                var routingKey = _incidentDetectionRoutingKeyBase.EndsWith(".")
                    ? $"{_incidentDetectionRoutingKeyBase}{_intersectionId}"
                    : $"{_incidentDetectionRoutingKeyBase}.{_intersectionId}";

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IIncidentDetectionPublisher>();

                    await publisher.PublishIncidentReportAsync(_intersectionId, incidentType, severity, description, timestamp);

                    _logger.LogInformation("[{Exchange}] Published incident detection with routing key '{RoutingKey}' for intersection {IntersectionId}: {Type} severity {Severity} at {Timestamp}",
                        _sensorDataExchange, routingKey, _intersectionId, incidentType, severity, timestamp);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing incident detection for intersection {IntersectionId}", _intersectionId);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Adjust frequency as needed
            }
        }
    }
}
