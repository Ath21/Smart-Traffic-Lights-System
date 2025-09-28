using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensorStore.Business;
using SensorStore.Models.Dtos;
using SensorStore.Models.Requests;

namespace SensorStore.Workers;

public class TrafficSensorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrafficSensorWorker> _logger;
    private readonly Random _rand = new();

    // Map physical intersections → stable identifiers
    private readonly Dictionary<int, string> _intersections = new()
    {
        { 1, "Agiou Spyridonos" },
        { 2, "Kentriki Pyli" },
        { 3, "Anatoliki Pyli" },
        { 4, "Dytiki Pyli" },
        { 5, "Ekklisia" }
    };

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

                foreach (var intersection in _intersections)
                {
                    var snapshot = new SensorSnapshotDto
                    {
                        IntersectionId = intersection.Key, // int IDs match DB + cache keys
                        VehicleCount = _rand.Next(5, 50),
                        PedestrianCount = _rand.Next(0, 20),
                        CyclistCount = _rand.Next(0, 10),
                        LastUpdated = DateTime.UtcNow
                    };

                    var avgSpeed = _rand.Next(20, 60); // km/h

                    // persist + cache + publish
                    await business.ReportSensorDataAsync(new SensorReportRequest
                    {
                        IntersectionId = snapshot.IntersectionId,
                        VehicleCount = snapshot.VehicleCount,
                        PedestrianCount = snapshot.PedestrianCount,
                        CyclistCount = snapshot.CyclistCount
                    });

                    _logger.LogInformation(
                        "Published sensor snapshot for {IntersectionName} (Id={IntersectionId})",
                        intersection.Value, intersection.Key
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TrafficSensorWorker loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
