using MassTransit;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Consumers;

public class VehicleCountConsumer : IConsumer<VehicleCountMessage>
{
    private readonly ILogger<VehicleCountConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;

    private const string ServiceTag = "[" + nameof(VehicleCountConsumer) + "]";

    public VehicleCountConsumer(
        ILogger<VehicleCountConsumer> logger,
        ITrafficAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // sensor.vehicle.count.{intersection_id}
    public async Task Consume(ConsumeContext<VehicleCountMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} VehicleCount received at Intersection {IntersectionId}: Count {Count}, AvgSpeed {Speed}",
            ServiceTag, msg.IntersectionId, msg.VehicleCount, msg.AvgSpeed);

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
