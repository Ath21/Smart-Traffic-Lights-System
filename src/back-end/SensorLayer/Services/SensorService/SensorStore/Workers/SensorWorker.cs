using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensorStore.Business;
using SensorStore.Domain;
using SensorStore.Models.Requests;

namespace SensorStore.Workers;

// ============================================================
// Sensor Worker (Fog Layer)
// ------------------------------------------------------------
// Simulates real-time traffic flow measurements for a given
// intersection. Generates random counts for vehicles,
// pedestrians, and cyclists, then stores them via business layer.
// ------------------------------------------------------------
// Published to: DetectionCacheDB (Redis) & DetectionDB (Mongo)
// ============================================================

public class SensorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorWorker> _logger;
    private readonly IntersectionContext _intersection;
    private readonly Random _rand = new();

    public SensorWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<SensorWorker> logger,
        IntersectionContext intersection)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intersection = intersection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[WORKER] SensorWorker started for {IntersectionName} (Id={IntersectionId})",
            _intersection.Name, _intersection.Id);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<ISensorBusiness>();

                // ============================================================
                // Generate vehicle counts
                // ============================================================
                var vehicleRequest = new VehicleCountRequest
                {
                    IntersectionId = _intersection.Id,
                    Intersection = _intersection.Name,
                    CountTotal = _rand.Next(10, 120),
                    AverageSpeedKmh = Math.Round(_rand.NextDouble() * 40 + 20, 1),
                    AverageWaitTimeSec = Math.Round(_rand.NextDouble() * 20, 1),
                    CountByDirection = new Dictionary<string, int>
                    {
                        ["north"] = _rand.Next(0, 30),
                        ["south"] = _rand.Next(0, 30),
                        ["east"]  = _rand.Next(0, 30),
                        ["west"]  = _rand.Next(0, 30)
                    },
                    VehicleBreakdown = new Dictionary<string, int>
                    {
                        ["car"]        = _rand.Next(10, 80),
                        ["bus"]        = _rand.Next(0, 10),
                        ["truck"]      = _rand.Next(0, 8),
                        ["motorcycle"] = _rand.Next(0, 15)
                    }
                };

                await business.RecordVehicleCountAsync(vehicleRequest);
                _logger.LogInformation("[WORKER] Vehicle count updated for {Intersection} (Total={Total})",
                    _intersection.Name, vehicleRequest.CountTotal);

                // ============================================================
                // Generate pedestrian counts
                // ============================================================
                var pedestrianRequest = new PedestrianCountRequest
                {
                    IntersectionId = _intersection.Id,
                    Intersection = _intersection.Name,
                    Count = _rand.Next(5, 80)
                };

                await business.RecordPedestrianCountAsync(pedestrianRequest);
                _logger.LogInformation("[WORKER] Pedestrian count updated for {Intersection} (Count={Count})",
                    _intersection.Name, pedestrianRequest.Count);

                // ============================================================
                // Generate cyclist counts
                // ============================================================
                var cyclistRequest = new CyclistCountRequest
                {
                    IntersectionId = _intersection.Id,
                    Intersection = _intersection.Name,
                    Count = _rand.Next(0, 20)
                };

                await business.RecordCyclistCountAsync(cyclistRequest);
                _logger.LogInformation("[WORKER] Cyclist count updated for {Intersection} (Count={Count})",
                    _intersection.Name, cyclistRequest.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[WORKER] Error in SensorWorker loop for {IntersectionName} (Id={IntersectionId})",
                    _intersection.Name, _intersection.Id);
            }

            // Wait 20 seconds before next cycle
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }

        _logger.LogInformation("[WORKER] SensorWorker stopped for {IntersectionName}", _intersection.Name);
    }
}
