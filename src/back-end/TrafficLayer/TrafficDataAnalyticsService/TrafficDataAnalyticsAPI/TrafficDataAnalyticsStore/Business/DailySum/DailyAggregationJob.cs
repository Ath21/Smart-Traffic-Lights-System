using System;
using TrafficDataAnalyticsData.Entities;
using TrafficDataAnalyticsStore.Repository.Congestion;
using TrafficDataAnalyticsStore.Repository.Summary;
using TrafficDataAnalyticsStore.Repository.Vehicle;

namespace TrafficDataAnalyticsStore.Business.DailySum;

public class DailyAggregationJob : BackgroundService
{
    private readonly ILogger<DailyAggregationJob> _logger;
    private readonly IVehicleCountRepository _vehicleCountRepository;
    private readonly IDailySummaryRepository _dailySummaryRepository;
    private readonly ICongestionAlertRepository _congestionAlertRepository;

    public DailyAggregationJob(
        ILogger<DailyAggregationJob> logger,
        IVehicleCountRepository vehicleCountRepository,
        IDailySummaryRepository dailySummaryRepository,
        ICongestionAlertRepository congestionAlertRepository)
    {
        _logger = logger;
        _vehicleCountRepository = vehicleCountRepository;
        _dailySummaryRepository = dailySummaryRepository;
        _congestionAlertRepository = congestionAlertRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);
                var intersectionIds = await _vehicleCountRepository.GetAllIntersectionIdsAsync();

                foreach (var id in intersectionIds)
                {
                    var counts = await _vehicleCountRepository.GetByIntersectionAndDateAsync(id, yesterday);
                    var totalCount = counts.Sum(c => c.Count);
                    var avgWait = counts.Any() ? 30f : 0f;

                    var summary = new DailySummary
                    {
                        SummaryId = Guid.NewGuid(),
                        IntersectionId = id,
                        Date = yesterday,
                        TotalVehicleCount = totalCount,
                        AverageWaitTime = avgWait,
                        PeakHours = "{}"
                    };

                    await _dailySummaryRepository.AddAsync(summary);

                    if (avgWait > 45 || totalCount > 10000)
                    {
                        var alert = new CongestionAlert
                        {
                            AlertId = Guid.NewGuid(),
                            IntersectionId = id,
                            Severity = "HIGH",
                            Description = $"High congestion detected at {id} on {yesterday:yyyy-MM-dd}",
                            Timestamp = DateTime.UtcNow
                        };

                        await _congestionAlertRepository.AddAsync(alert);
                    }
                }

                _logger.LogInformation("Daily aggregation job finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during aggregation job.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
