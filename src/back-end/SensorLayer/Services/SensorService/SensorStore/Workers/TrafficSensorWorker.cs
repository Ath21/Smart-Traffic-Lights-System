using SensorStore.Business;
using SensorStore.Domain;
using SensorStore.Models.Dtos;
using SensorStore.Models.Requests;

namespace SensorStore.Workers;

public class TrafficSensorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrafficSensorWorker> _logger;
    private readonly IntersectionContext _intersection;
    private readonly Random _rand = new();

    public TrafficSensorWorker(IServiceScopeFactory scopeFactory, ILogger<TrafficSensorWorker> logger, IntersectionContext intersection)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intersection = intersection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrafficSensorWorker started for {IntersectionName} (Id={IntersectionId})",
            _intersection.Name, _intersection.Id);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<ISensorCountService>();

                var snapshot = new SensorSnapshotDto
                {
                    IntersectionId = _intersection.Id,
                    VehicleCount = _rand.Next(5, 50),
                    PedestrianCount = _rand.Next(0, 20),
                    CyclistCount = _rand.Next(0, 10),
                    LastUpdated = DateTime.UtcNow
                };

                await business.ReportSensorDataAsync(new SensorReportRequest
                {
                    IntersectionId = snapshot.IntersectionId,
                    VehicleCount = snapshot.VehicleCount,
                    PedestrianCount = snapshot.PedestrianCount,
                    CyclistCount = snapshot.CyclistCount
                });

                _logger.LogInformation("Published snapshot for {IntersectionName}", _intersection.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TrafficSensorWorker loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
