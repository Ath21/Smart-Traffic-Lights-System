using DetectionData.TimeSeriesObjects;
using MassTransit;
using SensorMessages.Data;
using EmergencyVehicleDetectionStore.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace EmergencyVehicleDetectionStore.Consumers
{
    public class EmergencyVehicleDetectionConsumer : IConsumer<EmergencyVehicleDetectionMessage>
    {
        private readonly IEmergencyVehicleDetectionRepository _repository;
        private readonly ILogger<EmergencyVehicleDetectionConsumer> _logger;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKeyPattern;

        public EmergencyVehicleDetectionConsumer(
            IEmergencyVehicleDetectionRepository repository,
            ILogger<EmergencyVehicleDetectionConsumer> logger,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queueName = configuration["RabbitMQ:Queue:SensorEmergencyVehicleDetectionQueue"] ?? "sensor.emergency_vehicle_detection.queue";
            _exchangeName = configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _routingKeyPattern = configuration["RabbitMQ:RoutingKey:SensorEmergencyVehicleDetectionKey"] ?? "sensor.emergency_vehicle_detection.*";
        }

        public async Task Consume(ConsumeContext<EmergencyVehicleDetectionMessage> context)
        {
            var message = context.Message;

            _logger.LogInformation("Received emergency vehicle detection for intersection {IntersectionId}: Vehicle {VehicleId} of type {VehicleType} at {Timestamp}, Speed {Speed} km/h",
                message.IntersectionId, message.VehicleId, message.VehicleType, message.Timestamp, message.Speed);

            var detection = new EmergencyVehicleDetection
            {
                DetectionId = Guid.NewGuid(),
                IntersectionId = message.IntersectionId,
                Timestamp = message.Timestamp,
                Detected = true // since this message means an emergency vehicle was detected
            };

            // Insert the detection record to the DB
            await _repository.InsertAsync(detection);

            _logger.LogInformation("Stored emergency vehicle detection data to DB");
        }
    }
}
