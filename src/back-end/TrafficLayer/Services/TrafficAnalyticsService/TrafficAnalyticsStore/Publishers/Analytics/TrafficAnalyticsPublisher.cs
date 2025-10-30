using MassTransit;
using Messages.Traffic.Analytics;

namespace TrafficAnalyticsStore.Publishers.Analytics;

public class TrafficAnalyticsPublisher : ITrafficAnalyticsPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficAnalyticsPublisher> _logger;
    private readonly string _routingPattern;

    public TrafficAnalyticsPublisher(
        IConfiguration config,
        ILogger<TrafficAnalyticsPublisher> logger,
        IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:Analytics"]
                          ?? "traffic.analytics.#";
    }

    // ============================================================
    // CONGESTION ANALYTICS
    // ============================================================
    public async Task PublishCongestionAsync(CongestionAnalyticsMessage message)
    {
        var routingKey = BuildRoutingKey("congestion", message.Intersection);

        message.Timestamp = DateTime.UtcNow;

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "[PUBLISHER][ANALYTICS][{Intersection}] Congestion published: Level={Level:F2}, Status={Status}",
            message.Intersection, message.CongestionLevel, message.Status);
    }

    // ============================================================
    // INCIDENT ANALYTICS
    // ============================================================
    public async Task PublishIncidentAsync(IncidentAnalyticsMessage message)
    {
        var routingKey = BuildRoutingKey("incident", message.Intersection);

        message.Timestamp = DateTime.UtcNow;

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "[PUBLISHER][ANALYTICS][{Intersection}] Incident published: Type={Type}, Severity={Severity}",
            message.Intersection, message.IncidentType, message.Severity);
    }

    // ============================================================
    // SUMMARY ANALYTICS
    // ============================================================
    public async Task PublishSummaryAsync(SummaryAnalyticsMessage message)
    {
        var routingKey = BuildRoutingKey("summary", message.Intersection);

        message.GeneratedAt = DateTime.UtcNow;

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "[PUBLISHER][ANALYTICS][{Intersection}] Summary published: Vehicles={Vehicles}, Pedestrians={Pedestrians}, Cyclists={Cyclists}, Incidents={Incidents}, AvgCong={Cong:F2}",
            message.Intersection,
            message.VehicleCount,
            message.PedestrianCount,
            message.CyclistCount,
            message.IncidentsDetected,
            message.AverageCongestion);
    }

    // ============================================================
    // Helper: Build routing key for intersection/metric
    // ============================================================
    private string BuildRoutingKey(string metric, string intersection)
    {
        var baseKey = _routingPattern
            .TrimEnd('#', '.'); // remove any trailing wildcards or dots

        var safeIntersection = intersection
            .Replace(" ", "-")
            .Replace(".", "-")
            .ToLowerInvariant();

        var safeMetric = metric
            .Replace(" ", "-")
            .Replace(".", "-")
            .ToLowerInvariant();

        return $"{baseKey}.{safeIntersection}.{safeMetric}";
    }
}
