using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficDataAnalyticsStore.Models.Dtos;
using TrafficMessages;
using TrafficDataAnalyticsStore.Business;
using TrafficDataAnalyticsStore.Publishers.Summary;
using TrafficDataAnalyticsStore.Publishers.Congestion;

namespace TrafficDataAnalyticsStore.Consumers;

public class VehicleCountConsumer : IConsumer<VehicleCountMessage>
{
    private readonly ILogger<VehicleCountConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;
    private readonly ITrafficSummaryPublisher _summaryPublisher;
    private readonly ITrafficCongestionPublisher _congestionPublisher;

    public VehicleCountConsumer(
        ILogger<VehicleCountConsumer> logger,
        ITrafficAnalyticsService analyticsService,
        ITrafficSummaryPublisher summaryPublisher,
        ITrafficCongestionPublisher congestionPublisher)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _summaryPublisher = summaryPublisher;
        _congestionPublisher = congestionPublisher;
    }

    public async Task Consume(ConsumeContext<VehicleCountMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "VehicleCount received at Intersection {IntersectionId}: Count {Count}, AvgSpeed {Speed}",
            msg.IntersectionId, msg.VehicleCount, msg.AvgSpeed);

        var dto = new SummaryDto
        {
            IntersectionId = msg.IntersectionId,
            Date = msg.Timestamp.Date,
            AvgSpeed = msg.AvgSpeed,
            VehicleCount = msg.VehicleCount,
            CongestionLevel = InferCongestionLevel(msg.VehicleCount, msg.AvgSpeed)
        };

        await _analyticsService.AddOrUpdateSummaryAsync(dto);
    }


    private static string InferCongestionLevel(int vehicleCount, float avgSpeed)
    {
        if (avgSpeed < 10 || vehicleCount > 100) return "High";
        if (avgSpeed < 25 || vehicleCount > 50) return "Medium";
        return "Low";
    }
}
