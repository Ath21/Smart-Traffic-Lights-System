using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace UserStore.Consumers;

public class TrafficIncidentConsumer : IConsumer<TrafficIncidentMessage>
{
    private readonly ILogger<TrafficIncidentConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public TrafficIncidentConsumer(
        ILogger<TrafficIncidentConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _queueName = configuration["RabbitMQ:Queues:TrafficIncidentQueue"] ?? "traffic.analytics.incident.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:TrafficAnalyticsExchange"] ?? "traffic.analytics.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:TrafficIncident"] ?? "traffic.analytics.incident.*";
    }

    public Task Consume(ConsumeContext<TrafficIncidentMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation("Received TrafficIncident at Intersection {IntersectionId}: {Description}",
            msg.IntersectionId, msg.Description);

        // TODO: store incident or notify operators

        return Task.CompletedTask;
    }
}
