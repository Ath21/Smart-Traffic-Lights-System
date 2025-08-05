using System;
using MassTransit;
using SensorMessages.Data;

namespace VehicleDetectionStore.Workers;

public class VehicleSensor : BackgroundService
{
 private readonly IBus _bus;
    private readonly ILogger<VehicleSensor> _logger;
    private readonly Guid _intersectionId = Guid.NewGuid(); // Simulated intersection

    public VehicleSensor(IBus bus, ILogger<VehicleSensor> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new VehicleCountMessage(
                _intersectionId,
                random.Next(5, 50),        // Random vehicle count
                Math.Round(random.NextDouble() * 40 + 20, 2), // Avg speed 20–60 km/h
                DateTime.UtcNow
            );

            var routingKey = $"sensor.data.vehicle.count.{_intersectionId}";

            await _bus.Publish(message, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("Published vehicle count: {@Message}", message);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
