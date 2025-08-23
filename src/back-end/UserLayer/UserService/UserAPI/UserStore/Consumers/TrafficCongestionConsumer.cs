using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace UserStore.Consumers;

public class TrafficCongestionConsumer : IConsumer<TrafficCongestionMessage>
{
    private readonly ILogger<TrafficCongestionConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public TrafficCongestionConsumer(
        ILogger<TrafficCongestionConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _queueName = configuration["RabbitMQ:Queues:TrafficCongestionQueue"] ?? "traffic.analytics.congestion.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:TrafficAnalyticsExchange"] ?? "traffic.analytics.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:TrafficCongestion"] ?? "traffic.analytics.congestion.*";
    }

    public Task Consume(ConsumeContext<TrafficCongestionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation("Received TrafficCongestion at Intersection {IntersectionId} - Level {Level}: {Message}",
            msg.IntersectionId, msg.CongestionLevel, msg.Message);

        // TODO: trigger UI update or alert

        return Task.CompletedTask;
    }
}
