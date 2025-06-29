using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficDataAnalyticsStore.Models;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Publishers;

public class TrafficDataAnalyticsPublisher : ITrafficDataAnalyticsPublisher
{
    private readonly ILogger<TrafficDataAnalyticsPublisher> _logger;
    private readonly IBus _bus;
    private readonly string _analyticsExchangeName;
    private readonly string _summaryRoutingKey;
    private readonly string _alertRoutingKey;

    public TrafficDataAnalyticsPublisher(
        ILogger<TrafficDataAnalyticsPublisher> logger,
        IConfiguration configuration,
        IBus bus)
    {
        _logger = logger;
        _bus = bus;

        var section = configuration.GetSection("RabbitMQ");

        _analyticsExchangeName = section["TrafficAnalyticsExchange"] ?? "traffic.analytics";
        _summaryRoutingKey = section["RoutingKeys:DailySummary"] ?? "traffic.analytics.daily_summary";
        _alertRoutingKey = section["RoutingKeys:CongestionAlert"] ?? "traffic.analytics.congestion.alert";
    }

    public async Task PublishDailySummaryAsync(DailySummaryDto dto)
    {
        var message = new TrafficDailySummary(
            IntersectionId: dto.IntersectionId,
            VehicleCount: dto.TotalVehicleCount,
            AverageSpeed: 0, // TODO: Add logic for speed if available
            Notes: "Auto-generated summary",
            Timestamp: dto.Date.Date.AddHours(23).AddMinutes(59) // end of day snapshot
        );

        _logger.LogInformation("[SUMMARY] Publishing summary to {Exchange} with key {Key}", _analyticsExchangeName, _summaryRoutingKey);

        await _bus.Publish(message, context =>
        {
            context.SetRoutingKey(_summaryRoutingKey);
        });

        _logger.LogInformation("[SUMMARY] Published TrafficDailySummary for intersection {IntersectionId}", dto.IntersectionId);
    }

    public async Task PublishCongestionAlertAsync(CongestionAlertDto dto)
    {
        var message = new TrafficCongestionAlert(
            IntersectionId: dto.IntersectionId,
            CongestionLevel: dto.CongestionLevel,
            Severity: dto.Severity,
            Description: dto.Description,
            Timestamp: dto.Timestamp
        );

        _logger.LogInformation("[ALERT] Publishing alert to {Exchange} with key {Key}", _analyticsExchangeName, _alertRoutingKey);

        await _bus.Publish(message, context =>
        {
            context.SetRoutingKey(_alertRoutingKey);
        });

        _logger.LogInformation("[ALERT] Published TrafficCongestionAlert for intersection {IntersectionId}", dto.IntersectionId);
    }
}
