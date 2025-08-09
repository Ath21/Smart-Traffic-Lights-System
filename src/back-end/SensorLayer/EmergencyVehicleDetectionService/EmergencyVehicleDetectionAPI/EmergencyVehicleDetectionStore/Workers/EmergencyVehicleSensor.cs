using EmergencyVehicleDetectionStore.Publishers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SensorMessages.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmergencyVehicleDetectionStore.Workers
{
    public class EmergencyVehicleSensor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmergencyVehicleSensor> _logger;
        private readonly IConfiguration _configuration;
        private readonly Guid _intersectionId = Guid.NewGuid();

        private readonly string _sensorDataExchange;
        private readonly string _emergencyVehicleRoutingKeyBase;

        private readonly string[] _vehicleTypes = new[] { "ambulance", "fire_truck", "police_car" };
        private readonly string[] _vehicleIds = new[] { "EV-1001", "EV-1002", "EV-1003" };

        public EmergencyVehicleSensor(IServiceProvider serviceProvider, ILogger<EmergencyVehicleSensor> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _emergencyVehicleRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorEmergencyVehicleDetectionDetectionKey"] ?? "sensor.emergency_vehicle_detection.detection.*";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                // Randomly pick vehicle type and ID for simulation
                var vehicleType = _vehicleTypes[random.Next(_vehicleTypes.Length)];
                var vehicleId = _vehicleIds[random.Next(_vehicleIds.Length)];

                var speed = Math.Round(random.NextDouble() * 80 + 20, 2); // Speed between 20 and 100 km/h
                var timestamp = DateTime.UtcNow;

                var routingKey = _emergencyVehicleRoutingKeyBase.EndsWith(".")
                    ? $"{_emergencyVehicleRoutingKeyBase}{_intersectionId}"
                    : $"{_emergencyVehicleRoutingKeyBase}.{_intersectionId}";

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IEmergencyVehicleDetectionPublisher>();

                    await publisher.PublishEmergencyVehicleDetectionAsync(
                        _intersectionId,
                        vehicleId,
                        vehicleType,
                        speed,
                        timestamp
                    );

                    _logger.LogInformation(
                        "[{Exchange}] Published emergency vehicle detection with routing key '{RoutingKey}' for intersection {IntersectionId}: Vehicle {VehicleId} ({VehicleType}), Speed {Speed} km/h at {Timestamp}",
                        _sensorDataExchange, routingKey, _intersectionId, vehicleId, vehicleType, speed, timestamp
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing emergency vehicle detection for intersection {IntersectionId}", _intersectionId);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

    }
}
