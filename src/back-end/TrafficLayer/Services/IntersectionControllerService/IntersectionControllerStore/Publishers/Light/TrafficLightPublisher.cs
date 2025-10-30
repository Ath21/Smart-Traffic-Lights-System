using System;
using MassTransit;
using Messages.Traffic.Light;

namespace IntersectionControllerStore.Publishers.Light;

public class TrafficLightPublisher : ITrafficLightPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightPublisher> _logger;
    private readonly string _routingPattern;
    private readonly string _hostname;
    private readonly string _environment;

    public TrafficLightPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<TrafficLightPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        // Default routing: traffic.light.control.{intersection}.{light}
        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:LightControl"]
                          ?? "traffic.light.control.*.*";

        _hostname = Environment.MachineName;
        _environment = config["ASPNETCORE_ENVIRONMENT"] ?? "unknown";
    }

    // ============================================================
    // MAIN PUBLISH METHOD
    // ============================================================
    public async Task PublishLightControlAsync(TrafficLightControlMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.IntersectionName))
            throw new ArgumentException("IntersectionName must not be null or empty.");

        if (string.IsNullOrWhiteSpace(message.LightName))
            message.LightName = $"{message.IntersectionName}-Light";

        var routingKey = _routingPattern
            .Replace("{intersection}", SanitizeRoutingKeyPart(message.IntersectionName))
            .Replace("{light}", SanitizeRoutingKeyPart(message.LightName));

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "[PUBLISHER][TRAFFIC][LIGHT] Published LightControl for intersection={Intersection} light={Light} mode={Mode} env={Environment}",
            message.IntersectionName,
            message.LightName,
            message.Mode,
            _environment);
    }

    // ============================================================
    // INTERNAL HELPER
    // ============================================================
    private static string SanitizeRoutingKeyPart(string value)
        => value.Replace(" ", "-").Replace(".", "-").ToLowerInvariant();
}
