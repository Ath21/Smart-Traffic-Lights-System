using PedestrianDetectionStore.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PedestrianDetectionStore.Workers
{
    public class PedestrianSensor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PedestrianSensor> _logger;
        private readonly IConfiguration _configuration;
        private readonly Guid _intersectionId = Guid.NewGuid();

        private readonly string _sensorDataExchange;
        private readonly string _pedestrianDetectionRoutingKeyBase;

        public PedestrianSensor(IServiceProvider serviceProvider, ILogger<PedestrianSensor> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _pedestrianDetectionRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorPedestrianDetectionRequestKey"] ?? "sensor.pedestrian_detection.request.";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                var pedestrianCount = random.Next(1, 30);
                var timestamp = DateTime.UtcNow;

                var routingKey = _pedestrianDetectionRoutingKeyBase.EndsWith(".")
                    ? $"{_pedestrianDetectionRoutingKeyBase}{_intersectionId}"
                    : $"{_pedestrianDetectionRoutingKeyBase}.{_intersectionId}";

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IPedestrianDetectionPublisher>();

                    await publisher.PublishPedestrianRequestAsync(_intersectionId, pedestrianCount, timestamp);

                    _logger.LogInformation("[{Exchange}] Published pedestrian detection with routing key '{RoutingKey}' for intersection {IntersectionId}: {Count} pedestrians at {Timestamp}",
                        _sensorDataExchange, routingKey, _intersectionId, pedestrianCount, timestamp);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing pedestrian detection for intersection {IntersectionId}", _intersectionId);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
