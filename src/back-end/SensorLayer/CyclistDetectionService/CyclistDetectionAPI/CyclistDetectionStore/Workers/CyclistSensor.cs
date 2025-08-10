using CyclistDetectionStore.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyclistDetectionStore.Workers
{
    public class CyclistSensor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CyclistSensor> _logger;
        private readonly IConfiguration _configuration;
        private readonly Guid _intersectionId = Guid.NewGuid();

        private readonly string _sensorDataExchange;
        private readonly string _cyclistDetectionRoutingKeyBase;

        public CyclistSensor(IServiceProvider serviceProvider, ILogger<CyclistSensor> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            _sensorDataExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange";
            _cyclistDetectionRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:SensorCyclistDetectionRequestKey"] ?? "sensor.cyclist_detection.request.";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                var cyclistCount = random.Next(1, 20);
                var avgSpeed = random.NextDouble() * (30.0 - 5.0) + 5.0; // Speed between 5 and 30
                var timestamp = DateTime.UtcNow;

                var routingKey = _cyclistDetectionRoutingKeyBase.EndsWith(".")
                    ? $"{_cyclistDetectionRoutingKeyBase}{_intersectionId}"
                    : $"{_cyclistDetectionRoutingKeyBase}.{_intersectionId}";

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<ICyclistDetectionPublisher>();

                    await publisher.PublishCyclistRequestAsync(_intersectionId, cyclistCount, avgSpeed, timestamp);

                    _logger.LogInformation("[{Exchange}] Published cyclist detection with routing key '{RoutingKey}' for intersection {IntersectionId}: {Count} cyclists at {Timestamp}, AvgSpeed {AvgSpeed} km/h",
                        _sensorDataExchange, routingKey, _intersectionId, cyclistCount, timestamp, avgSpeed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing cyclist detection for intersection {IntersectionId}", _intersectionId);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
