using DetectionData.TimeSeriesObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SensorMessages.Data;
using IncidentDetectionStore.Repositories;
using System;
using System.Threading.Tasks;

namespace IncidentDetectionStore.Consumers
{
    public class IncidentDetectionConsumer : IConsumer<IncidentDetectionMessage>
    {
        private readonly IIncidentDetectionRepository _repository;
        private readonly ILogger<IncidentDetectionConsumer> _logger;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKeyPattern;

        public IncidentDetectionConsumer(
            IIncidentDetectionRepository repository,
            ILogger<IncidentDetectionConsumer> logger,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queueName = configuration["RabbitMQ:Queue:SensorIncidentDetectionReportQueue"]
                ?? "sensor.incident_detection.report.queue";
            _exchangeName = configuration["RabbitMQ:Exchange:SensorDataExchange"]
                ?? "sensor.data.exchange";
            _routingKeyPattern = configuration["RabbitMQ:RoutingKey:SensorIncidentDetectionReportKey"]
                ?? "sensor.incident_detection.report.*";
        }

        public async Task Consume(ConsumeContext<IncidentDetectionMessage> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received incident report for intersection {IntersectionId}: Type={IncidentType}, Severity={Severity}, Description={Description}, Timestamp={Timestamp}",
                message.IntersectionId, message.IncidentType, message.Severity, message.Description, message.Timestamp);

            var detection = new IncidentDetection
            {
                IntersectionId = message.IntersectionId,
                Timestamp = message.Timestamp,
                Description = message.Description
            };

            await _repository.InsertAsync(detection);

            _logger.LogInformation("Stored incident detection data to DB");
        }
    }
}
