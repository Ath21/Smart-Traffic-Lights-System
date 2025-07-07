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
        if (string.IsNullOrWhiteSpace(dto.IntersectionId))
        {
            _logger.LogWarning("[SUMMARY] Skipped publish: IntersectionId is missing.");
            return;
        }

        var message = new TrafficDailySummary(
            IntersectionId: dto.IntersectionId,
            VehicleCount: dto.TotalVehicleCount,
            AverageSpeed: 0, // TODO: Replace with actual calculation if available
            Notes: $"Summary for {dto.IntersectionId}",
            Timestamp: dto.Date.Date.AddHours(23).AddMinutes(59)
        );

        _logger.LogInformation("[SUMMARY] Publishing to {Exchange} with routing key {Key}", _analyticsExchangeName, _summaryRoutingKey);

        await _bus.Publish(message, context =>
        {
            context.SetRoutingKey(_summaryRoutingKey);
        });

        _logger.LogInformation("[SUMMARY] Published for {IntersectionId}", dto.IntersectionId);
    }

    public async Task PublishCongestionAlertAsync(CongestionAlertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IntersectionId))
        {
            _logger.LogWarning("[ALERT] Skipped publish: IntersectionId is missing.");
            return;
        }

        var now = DateTime.UtcNow;

        var message = new TrafficCongestionAlert(
            IntersectionId: dto.IntersectionId,
            CongestionLevel: GetCongestionLevelFromSeverity(dto.Severity),
            Severity: dto.Severity,
            Description: $"Detected {dto.Severity} congestion at intersection {dto.IntersectionId}",
            Timestamp: now
        );

        var routingKey = $"{_alertRoutingKey}.{dto.IntersectionId}";

        _logger.LogInformation("[ALERT] Publishing to {Exchange} with routing key {Key}", _analyticsExchangeName, routingKey);

        await _bus.Publish(message, context =>
        {
            context.SetRoutingKey(routingKey);
        });

        _logger.LogInformation("[ALERT] Published for {IntersectionId}", dto.IntersectionId);
    }

    private static double GetCongestionLevelFromSeverity(string severity) => severity switch
    {
        "HIGH" => 0.9,
        "MEDIUM" => 0.6,
        _ => 0.3
    };
}
