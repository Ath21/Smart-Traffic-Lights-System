using DetectionData.TimeSeriesObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SensorMessages.Data;
using PedestrianDetectionStore.Repositories;
using System;
using System.Threading.Tasks;

namespace PedestrianDetectionStore.Consumers
{
    public class PedestrianDetectionConsumer : IConsumer<PedestrianDetectionMessage>
    {
        private readonly IPedestrianDetectionRepository _repository;
        private readonly ILogger<PedestrianDetectionConsumer> _logger;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKeyPattern;

        public PedestrianDetectionConsumer(
            IPedestrianDetectionRepository repository,
            ILogger<PedestrianDetectionConsumer> logger,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queueName = configuration["RabbitMQ:Queue:SensorPedestrianDetectionRequestQueue"]
                ?? "sensor.pedestrian_detection.request.queue";
            _exchangeName = configuration["RabbitMQ:Exchange:SensorDataExchange"]
                ?? "sensor.data.exchange";
            _routingKeyPattern = configuration["RabbitMQ:RoutingKey:SensorPedestrianDetectionRequestKey"]
                ?? "sensor.pedestrian_detection.request.*";
        }

        public async Task Consume(ConsumeContext<PedestrianDetectionMessage> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received pedestrian detection for intersection {IntersectionId}: {Count} pedestrians at {Timestamp}",
                message.IntersectionId, message.PedestrianCount, message.Timestamp);

            var detection = new PedestrianDetection
            {
                IntersectionId = message.IntersectionId,
                Count = message.PedestrianCount,
                Timestamp = message.Timestamp
            };

            await _repository.InsertAsync(detection);

            _logger.LogInformation("Stored pedestrian detection data to DB");
        }
    }
}
