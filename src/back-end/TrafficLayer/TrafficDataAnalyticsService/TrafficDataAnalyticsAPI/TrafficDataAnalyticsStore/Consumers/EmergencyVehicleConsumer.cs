using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace TrafficDataAnalyticsStore.Consumers;

public class EmergencyVehicleConsumer : IConsumer<EmergencyVehicleMessage>
{
    private readonly ILogger<EmergencyVehicleConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public EmergencyVehicleConsumer(ILogger<EmergencyVehicleConsumer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _queueName = configuration["RabbitMQ:Queues:EmergencyVehicleQueue"] ?? "sensor.vehicle.emergency.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:SensorExchange"] ?? "sensor.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:EmergencyVehicle"] ?? "sensor.vehicle.emergency.*";
    }

    public Task Consume(ConsumeContext<EmergencyVehicleMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "Emergency Vehicle {Detected} at Intersection {IntersectionId}",
            msg.Detected ? "Detected" : "Not Detected", msg.IntersectionId);

        return Task.CompletedTask;
    }
}
