using DetectionStore.Business;
using DetectionStore.Models.Dtos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DetectionStore.Workers;

public class DetectionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DetectionWorker> _logger;
    private readonly Random _rand = new();

    public DetectionWorker(IServiceScopeFactory scopeFactory, ILogger<DetectionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DetectionWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<ISensorDetectionService>();

                var intersectionId = Guid.NewGuid();

                // Emergency vehicle detection ~10% chance
                if (_rand.NextDouble() < 0.1)
                {
                    var emergency = new EmergencyVehicleDto
                    {
                        IntersectionId = intersectionId,
                        Detected = true,
                        Type = _rand.NextDouble() < 0.5 ? "ambulance" : "firetruck",
                        PriorityLevel = 1,
                        Timestamp = DateTime.UtcNow
                    };

                    await business.RecordEmergencyAsync(emergency);
                    _logger.LogInformation("Emergency vehicle detected at {Intersection}", intersectionId);
                }

                // Public transport detection ~30% chance
                if (_rand.NextDouble() < 0.3)
                {
                    var transport = new PublicTransportDto
                    {
                        IntersectionId = intersectionId,
                        Detected = true,
                        Mode = _rand.NextDouble() < 0.5 ? "bus" : "tram",
                        RouteId = $"R{_rand.Next(1, 50)}",
                        Timestamp = DateTime.UtcNow
                    };

                    await business.RecordPublicTransportAsync(transport);
                    _logger.LogInformation(
                        "Public transport detected at {Intersection}, Route {Route}",
                        intersectionId, transport.RouteId);
                }

                // Incident detection ~5% chance
                if (_rand.NextDouble() < 0.05)
                {
                    var incident = new IncidentDto
                    {
                        IntersectionId = intersectionId,
                        Type = "collision",
                        Severity = _rand.Next(1, 5),
                        Description = "Random simulated incident",
                        Timestamp = DateTime.UtcNow
                    };

                    await business.RecordIncidentAsync(incident);
                    _logger.LogWarning("Incident detected at {Intersection}: {Desc}", intersectionId, incident.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DetectionWorker loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // every 30s
        }
    }
}
