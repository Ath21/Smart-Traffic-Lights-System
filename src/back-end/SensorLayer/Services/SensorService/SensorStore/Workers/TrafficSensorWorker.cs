using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensorStore.Business;
using SensorStore.Models.Dtos;

namespace SensorStore.Workers;

public class TrafficSensorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrafficSensorWorker> _logger;
    private readonly Random _rand = new();

    public TrafficSensorWorker(IServiceScopeFactory scopeFactory, ILogger<TrafficSensorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrafficSensorWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<ISensorCountService>();

                // Simulate 3 intersections
                foreach (var intersectionId in new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() })
                {
                    var snapshot = new SensorSnapshotDto
                    {
                        IntersectionId = intersectionId,
                        VehicleCount = _rand.Next(5, 50),       // vehicles per cycle
                        PedestrianCount = _rand.Next(0, 20),    // pedestrians
                        CyclistCount = _rand.Next(0, 10),       // cyclists
                        LastUpdated = DateTime.UtcNow
                    };

                    var avgSpeed = _rand.Next(20, 60); // km/h
                    await business.UpdateSnapshotAsync(snapshot, avgSpeed);

                    _logger.LogInformation("Published sensor snapshot for {Intersection}", intersectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TrafficSensorWorker loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); // every 15s
        }
    }
}
