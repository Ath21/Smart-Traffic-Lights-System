using System;
using TrafficDataAnalyticsData.Entities;
using TrafficDataAnalyticsStore.Repository.Summary;
using TrafficDataAnalyticsStore.Repository.Vehicle;

namespace TrafficDataAnalyticsStore.Business.DailySum;

public class DailyAggregationService : IDailyAggregationService
{
    private readonly IVehicleCountRepository _vehicleCountRepository;
    private readonly IDailySummaryRepository _dailySummaryRepository;
    
    public DailyAggregationService(
        IVehicleCountRepository vehicleCountRepository,
        IDailySummaryRepository dailySummaryRepository
    )
    {
        _vehicleCountRepository = vehicleCountRepository;
        _dailySummaryRepository = dailySummaryRepository;
    }

    public async Task GenerateSummaryAsync(string intersectionId, DateTime date)
    {
        var counts = await _vehicleCountRepository.GetByIntersectionAndDateAsync(intersectionId, date);
        var total = counts.Sum(c => c.Count);

        var summary = new DailySummary
        {
            SummaryId = Guid.NewGuid(),
            IntersectionId = intersectionId,
            Date = date,
            TotalVehicleCount = total,
            AverageWaitTime = total > 0 ? 30f : 0f,
            PeakHours = "{}"
        };

        await _dailySummaryRepository.AddAsync(summary);
    }
}
