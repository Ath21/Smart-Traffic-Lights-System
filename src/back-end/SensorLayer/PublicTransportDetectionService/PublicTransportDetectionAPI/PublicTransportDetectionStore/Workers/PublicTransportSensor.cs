using PublicTransportDetectionStore.Publishers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PublicTransportDetectionStore.Workers
{
    public class PublicTransportSensor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PublicTransportSensor> _logger;
        private readonly IConfiguration _configuration;
        private readonly Guid _intersectionId = Guid.NewGuid();

        private readonly string _sensorDataExchange;
        private readonly string _publicTransportRoutingKeyBase;

        private readonly string[] _routeIds = new[] { "bus_10", "tram_5", "bus_15" };
        private readonly string[] _vehicleTypes = new[] { "bus", "tram" };

        private readonly Random _random = new();

        public PublicTransportSensor(IServiceProvider serviceProvider, ILogger<PublicTransportSensor> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _publicTransportRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorPublicTransportDetectionDetectionKey"] 
                ?? "sensor.public_transport_detection.detection.*";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var routeId = _routeIds[_random.Next(_routeIds.Length)];
                var vehicleType = _vehicleTypes[_random.Next(_vehicleTypes.Length)];
                var passengerCount = _random.Next(0, 80); // simulate passenger count between 0-80
                var timestamp = DateTime.UtcNow;

                var routingKey = _publicTransportRoutingKeyBase.TrimEnd('*').TrimEnd('.') + "." + _intersectionId;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IPublicTransportDetectionPublisher>();

                    await publisher.PublishPublicTransportDetectionAsync(
                        _intersectionId,
                        routeId,
                        vehicleType,
                        passengerCount,
                        timestamp
                    );

                    _logger.LogInformation(
                        "[{Exchange}] Published public transport detection with routing key '{RoutingKey}' for intersection {IntersectionId}: Route {RouteId}, VehicleType {VehicleType}, Passengers {PassengerCount} at {Timestamp}",
                        _sensorDataExchange, routingKey, _intersectionId, routeId, vehicleType, passengerCount, timestamp
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing public transport detection for intersection {IntersectionId}", _intersectionId);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
