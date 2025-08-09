using DetectionData.TimeSeriesObjects;
using MassTransit;

using SensorMessages.Data;
using VehicleDetectionStore.Repositories;

namespace VehicleDetectionStore.Consumers
{
    public class VehicleCountConsumer : IConsumer<VehicleCountMessage>
    {
        private readonly IVehicleDetectionRepository _repository;
        private readonly ILogger<VehicleCountConsumer> _logger;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKeyPattern;

        public VehicleCountConsumer(
            IVehicleDetectionRepository repository, 
            ILogger<VehicleCountConsumer> logger,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queueName = configuration["RabbitMQ:Queue:SensorVehicleDetectionVehicleCountQueue"] ?? "sensor.vehicle_detection.vehicle_count.queue";
            _exchangeName = configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _routingKeyPattern = configuration["RabbitMQ:RoutingKey:SensorVehicleDetectionVehicleCountKey"] ?? "sensor.vehicle_detection.vehicle_count.*";
        }

        public async Task Consume(ConsumeContext<VehicleCountMessage> context)
        {
            var message = context.Message;

            _logger.LogInformation("Received vehicle count for intersection {IntersectionId}: {Count} vehicles at {Timestamp}",
                message.IntersectionId, message.VehicleCount, message.Timestamp);

            var detection = new VehicleDetection
            {
                IntersectionId = message.IntersectionId,
                VehicleCount = message.VehicleCount,
                AvgSpeed = (float)message.AvgSpeed,
                Timestamp = message.Timestamp
            };

            // Insert the detection record to InfluxDB time-series database
            await _repository.InsertAsync(detection);

            _logger.LogInformation("Stored vehicle detection data to DB");

            // Additional logging or error handling can be added here if needed
        }
    }
}
