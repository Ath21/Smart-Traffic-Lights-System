using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace UserStore.Consumers;

public class TrafficSummaryConsumer : IConsumer<TrafficSummaryMessage>
{
    private readonly ILogger<TrafficSummaryConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public TrafficSummaryConsumer(
        ILogger<TrafficSummaryConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _queueName = configuration["RabbitMQ:Queues:TrafficSummaryQueue"] ?? "traffic.analytics.summary.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:TrafficAnalyticsExchange"] ?? "traffic.analytics.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:TrafficSummary"] ?? "traffic.analytics.summary.*";
    }

    public Task Consume(ConsumeContext<TrafficSummaryMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation("Received TrafficSummary for Intersection {IntersectionId} - {VehicleCount} vehicles, AvgSpeed {AvgSpeed}, Congestion {Level}",
            msg.IntersectionId, msg.VehicleCount, msg.AvgSpeed, msg.CongestionLevel);

        // TODO: persist or visualize in dashboards

        return Task.CompletedTask;
    }
}
