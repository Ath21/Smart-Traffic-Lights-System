using System;
using TrafficDataAnalyticsData.Entities;
using TrafficDataAnalyticsStore.Business.CongestionDetection;
using TrafficDataAnalyticsStore.Models;
using TrafficDataAnalyticsStore.Publishers;
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
    private readonly ICongestionDetector _congestionDetector;
    private readonly ITrafficDataAnalyticsPublisher _publisher;

    public DailyAggregationJob(
        ILogger<DailyAggregationJob> logger,
        IVehicleCountRepository vehicleCountRepository,
        IDailySummaryRepository dailySummaryRepository,
        ICongestionAlertRepository congestionAlertRepository,
        ICongestionDetector congestionDetector,
        ITrafficDataAnalyticsPublisher publisher)
    {
        _logger = logger;
        _vehicleCountRepository = vehicleCountRepository;
        _dailySummaryRepository = dailySummaryRepository;
        _congestionAlertRepository = congestionAlertRepository;
        _congestionDetector = congestionDetector;
        _publisher = publisher;
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

                    var summaryDto = new DailySummaryDto
                    {
                        IntersectionId = summary.IntersectionId,
                        Date = summary.Date,
                        AvgWaitTime = summary.AverageWaitTime,
                        PeakHours = summary.PeakHours,
                        TotalVehicleCount = summary.TotalVehicleCount
                    };

                    await _publisher.PublishDailySummaryAsync(summaryDto);
                    _logger.LogInformation("Published daily summary for intersection {IntersectionId} on {Date}", id, yesterday);


                    if (_congestionDetector.isCongested(avgWait, totalCount))
                    {
                        var severity = _congestionDetector.GetSeverity(avgWait, totalCount);

                        var alert = new CongestionAlert
                        {
                            AlertId = Guid.NewGuid(),
                            IntersectionId = id,
                            Severity = severity,
                            Description = $"High congestion detected at {id} on {yesterday:yyyy-MM-dd}",
                            Timestamp = DateTime.UtcNow
                        };

                        await _congestionAlertRepository.AddAsync(alert);

                        var alertDto = new CongestionAlertDto
                        {
                            IntersectionId = alert.IntersectionId,
                            Severity = alert.Severity
                        };

                        await _publisher.PublishCongestionAlertAsync(alertDto);
                        _logger.LogInformation("Published congestion alert for intersection {IntersectionId} with severity {Severity}", id, severity);
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
