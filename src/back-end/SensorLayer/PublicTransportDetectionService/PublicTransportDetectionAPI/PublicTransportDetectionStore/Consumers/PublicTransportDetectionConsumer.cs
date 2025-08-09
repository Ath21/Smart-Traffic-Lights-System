using DetectionData.TimeSeriesObjects;
using MassTransit;
using PublicTransportDetectionStore.Repositories;
using PublicTransportDetectionStore.Publishers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using SensorMessages.Data;

namespace PublicTransportDetectionStore.Consumers
{
    public class PublicTransportDetectionConsumer : IConsumer<PublicTransportDetectionMessage>
    {
        private readonly IPublicTransportDetectionRepository _repository;
        private readonly ILogger<PublicTransportDetectionConsumer> _logger;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKeyPattern;

        public PublicTransportDetectionConsumer(
            IPublicTransportDetectionRepository repository,
            ILogger<PublicTransportDetectionConsumer> logger,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queueName = configuration["RabbitMQ:Queue:SensorPublicTransportDetectionQueue"] ?? "sensor.public_transport_detection.detection.queue";
            _exchangeName = configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _routingKeyPattern = configuration["RabbitMQ:RoutingKey:SensorPublicTransportDetectionDetectionKey"] ?? "sensor.public_transport_detection.detection.*";
        }

        public async Task Consume(ConsumeContext<PublicTransportDetectionMessage> context)
        {
            var message = context.Message;

            _logger.LogInformation("Received public transport detection at intersection {IntersectionId}: Route {RouteId}, VehicleType {VehicleType}, PassengerCount {PassengerCount} at {Timestamp}",
                message.IntersectionId, message.RouteId, message.VehicleType, message.PassengerCount, message.Timestamp);

            var detection = new PublicTransportDetection
            {
                DetectionId = Guid.NewGuid(),
                IntersectionId = message.IntersectionId,
                RouteId = message.RouteId,
                Timestamp = message.Timestamp
            };

            await _repository.InsertAsync(detection);

            _logger.LogInformation("Stored public transport detection data to DB");
        }
    }
}
