using DetectionStore.Business;
using DetectionStore.Domain;
using DetectionStore.Models.Requests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DetectionStore.Workers;

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
            "DetectionWorker started for {IntersectionName} (Id={IntersectionId})",
            _intersection.Name, _intersection.Id);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<IDetectionEventService>();

                // Emergency vehicle detection ~10% chance
                if (_rand.NextDouble() < 0.1)
                {
                    var request = new DetectionEventRequest
                    {
                        EventType = "emergency",
                        Detected = true,
                        Description = _rand.NextDouble() < 0.5 ? "ambulance" : "firetruck"
                    };

                    await business.ReportEventAsync(request);
                }

                // Public transport detection ~30% chance
                if (_rand.NextDouble() < 0.3)
                {
                    var request = new DetectionEventRequest
                    {
                        EventType = "public_transport",
                        Detected = true,
                        Description = $"Route R{_rand.Next(1, 50)}"
                    };

                    await business.ReportEventAsync(request);
                }

                // Incident detection ~5% chance
                if (_rand.NextDouble() < 0.05)
                {
                    var request = new DetectionEventRequest
                    {
                        EventType = "incident",
                        Detected = true,
                        Description = "Random simulated incident"
                    };

                    await business.ReportEventAsync(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in DetectionWorker loop for {IntersectionName} (Id={IntersectionId})",
                    _intersection.Name, _intersection.Id);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
