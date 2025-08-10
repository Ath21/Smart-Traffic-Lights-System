using DetectionData.TimeSeriesObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SensorMessages.Data;
using CyclistDetectionStore.Repositories;
using System;
using System.Threading.Tasks;

namespace CyclistDetectionStore.Consumers
{
    public class CyclistDetectionConsumer : IConsumer<CyclistDetectionMessage>
    {
        private readonly ICyclistDetectionRepository _repository;
        private readonly ILogger<CyclistDetectionConsumer> _logger;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKeyPattern;

        public CyclistDetectionConsumer(
            ICyclistDetectionRepository repository,
            ILogger<CyclistDetectionConsumer> logger,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queueName = configuration["RabbitMQ:Queue:SensorCyclistDetectionRequestQueue"]
                ?? "sensor.cyclist_detection.request.queue";
            _exchangeName = configuration["RabbitMQ:Exchange:SensorDataExchange"]
                ?? "sensor.data.exchange";
            _routingKeyPattern = configuration["RabbitMQ:RoutingKey:SensorCyclistDetectionRequestKey"]
                ?? "sensor.cyclist_detection.request.*";
        }

        public async Task Consume(ConsumeContext<CyclistDetectionMessage> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received cyclist detection for intersection {IntersectionId}: {Count} cyclists at avg speed {AvgSpeed} at {Timestamp}",
                message.IntersectionId, message.CyclistCount, message.AvgSpeed, message.Timestamp);

            var detection = new CyclistDetection
            {
                IntersectionId = message.IntersectionId,
                Count = message.CyclistCount,
                Timestamp = message.Timestamp
            };

            await _repository.InsertAsync(detection);

            _logger.LogInformation("Stored cyclist detection data to DB");
        }
    }
}
