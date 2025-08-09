using VehicleDetectionStore.Publishers;

namespace VehicleDetectionStore.Workers
{
    public class VehicleSensor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VehicleSensor> _logger;
        private readonly IConfiguration _configuration;
        private readonly Guid _intersectionId = Guid.NewGuid();

        private readonly string _sensorDataExchange;
        private readonly string _vehicleCountRoutingKeyBase;

        public VehicleSensor(IServiceProvider serviceProvider, ILogger<VehicleSensor> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _vehicleCountRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorVehicleDetectionVehicleCountKey"] ?? "sensor.vehicle_detection.vehicle_count.";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                var vehicleCount = random.Next(5, 50);
                var avgSpeed = Math.Round(random.NextDouble() * 40 + 20, 2);
                var timestamp = DateTime.UtcNow;

                var routingKey = _vehicleCountRoutingKeyBase.EndsWith(".")
                    ? $"{_vehicleCountRoutingKeyBase}{_intersectionId}"
                    : $"{_vehicleCountRoutingKeyBase}.{_intersectionId}";

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IVehicleDetectionPublisher>();

                    await publisher.PublishVehicleCountAsync(_intersectionId, vehicleCount, avgSpeed, timestamp);

                    _logger.LogInformation("[{Exchange}] Published vehicle count with routing key '{RoutingKey}' for intersection {IntersectionId}: {Count} vehicles, {Speed} km/h avg at {Timestamp}",
                        _sensorDataExchange, routingKey, _intersectionId, vehicleCount, avgSpeed, timestamp);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing vehicle count for intersection {IntersectionId}", _intersectionId);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
