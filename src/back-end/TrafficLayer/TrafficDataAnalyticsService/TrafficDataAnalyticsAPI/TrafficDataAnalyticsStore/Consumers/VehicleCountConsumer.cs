using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace TrafficDataAnalyticsStore.Consumers;

public class VehicleCountConsumer : IConsumer<VehicleCountMessage>
{
    private readonly ILogger<VehicleCountConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public VehicleCountConsumer(ILogger<VehicleCountConsumer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _queueName = configuration["RabbitMQ:Queues:VehicleCountQueue"] ?? "sensor.vehicle.count.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:SensorExchange"] ?? "sensor.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:VehicleCount"] ?? "sensor.vehicle.count.*";
    }

    public Task Consume(ConsumeContext<VehicleCountMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "VehicleCount received at Intersection {IntersectionId} - Count {Count}, AvgSpeed {AvgSpeed}",
            msg.IntersectionId, msg.VehicleCount, msg.AvgSpeed);

        // TODO: aggregate into TrafficDataDbContext

        return Task.CompletedTask;
    }
}
