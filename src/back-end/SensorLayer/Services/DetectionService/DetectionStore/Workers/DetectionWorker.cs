using DetectionStore.Business;
using DetectionStore.Domain;
using DetectionStore.Models.Requests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DetectionStore.Workers;

// ============================================================
// Detection Worker (Sensor Layer)
// ------------------------------------------------------------
// Simulates random detection events for a specific intersection.
// Publishes data via DetectionBusiness → RabbitMQ topics
//    sensor.detection.{intersection}.{event}
// ============================================================

public class DetectionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DetectionWorker> _logger;
    private readonly IntersectionContext _intersection;
    private readonly Random _rand = new();

    public DetectionWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<DetectionWorker> logger,
        IntersectionContext intersection)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intersection = intersection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[WORKER] DetectionWorker started for {IntersectionName} (Id={IntersectionId})",
            _intersection.Name, _intersection.Id);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<IDetectionBusiness>();

                // ------------------------------------------------------------
                // Simulate emergency vehicle detections (~10% probability)
                // ------------------------------------------------------------
                if (_rand.NextDouble() < 0.1)
                {
                    var request = new EmergencyVehicleDetectionRequest
                    {
                        IntersectionId = _intersection.Id,
                        Intersection = _intersection.Name,
                        Direction = GetRandomDirection(),
                        EmergencyVehicleType = _rand.NextDouble() < 0.5 ? "ambulance" : "firetruck",
                        DetectedAt = DateTime.UtcNow
                    };

                    await business.CreateEmergencyAsync(request);
                    _logger.LogInformation("[WORKER] Emergency vehicle detected ({Type}) at {Intersection}",
                        request.EmergencyVehicleType, _intersection.Name);
                }

                // ------------------------------------------------------------
                // Simulate public transport detections (~30% probability)
                // ------------------------------------------------------------
                if (_rand.NextDouble() < 0.3)
                {
                    var request = new PublicTransportDetectionRequest
                    {
                        IntersectionId = _intersection.Id,
                        IntersectionName = _intersection.Name,
                        LineName = $"Bus{_rand.Next(700, 899)}",
                        DetectedAt = DateTime.UtcNow
                    };

                    await business.CreatePublicTransportAsync(request);
                    _logger.LogInformation("[WORKER] Public transport detected ({Line}) at {Intersection}",
                        request.LineName, _intersection.Name);
                }

                // ------------------------------------------------------------
                // Simulate random incidents (~5% probability)
                // ------------------------------------------------------------
                if (_rand.NextDouble() < 0.05)
                {
                    var request = new IncidentDetectionRequest
                    {
                        IntersectionId = _intersection.Id,
                        Intersection = _intersection.Name,
                        Description = "Random simulated traffic incident",
                        ReportedAt = DateTime.UtcNow
                    };

                    await business.CreateIncidentAsync(request);
                    _logger.LogWarning("[WORKER] Incident reported at {Intersection}: {Description}",
                        _intersection.Name, request.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[WORKER] Error in DetectionWorker loop for {IntersectionName} (Id={IntersectionId})",
                    _intersection.Name, _intersection.Id);
            }

            // Wait between simulation cycles
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("[WORKER] DetectionWorker stopped for {IntersectionName}", _intersection.Name);
    }

    private static string GetRandomDirection()
    {
        var directions = new[] { "north", "south", "east", "west" };
        return directions[Random.Shared.Next(directions.Length)];
    }
}
