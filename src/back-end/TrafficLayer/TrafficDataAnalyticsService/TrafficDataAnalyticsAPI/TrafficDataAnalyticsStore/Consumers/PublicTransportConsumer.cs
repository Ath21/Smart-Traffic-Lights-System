using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace TrafficDataAnalyticsStore.Consumers;

public class PublicTransportConsumer : IConsumer<PublicTransportMessage>
{
    private readonly ILogger<PublicTransportConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public PublicTransportConsumer(ILogger<PublicTransportConsumer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _queueName = configuration["RabbitMQ:Queues:PublicTransportQueue"] ?? "sensor.public_transport.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:SensorExchange"] ?? "sensor.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:PublicTransport"] ?? "sensor.public_transport.request.*";
    }

    public Task Consume(ConsumeContext<PublicTransportMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "PublicTransport request at Intersection {IntersectionId}, Route {RouteId}",
            msg.IntersectionId, msg.RouteId ?? "N/A");

        return Task.CompletedTask;
    }
}
