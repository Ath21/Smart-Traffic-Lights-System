using MassTransit;
using Messages.Traffic;
using IntersectionControllerStore.Domain;

namespace IntersectionControllerStore.Publishers.Light;

public class TrafficLightControlPublisher : ITrafficLightControlPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightControlPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public TrafficLightControlPublisher(
        IConfiguration config,
        ILogger<TrafficLightControlPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:Control"]
                          ?? "traffic.light.control.{intersection}.{light}";
    }

    // ============================================================
    // Publish control command for a specific light
    // ============================================================
    public async Task PublishLightControlAsync(
        int lightId,
        string lightName,
        string operationalMode,
        string timePlan,
        Dictionary<string, int> phaseDurations,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        var msg = new TrafficLightControlMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            LightId = lightId,
            LightName = lightName,
            OperationalMode = operationalMode,
            TimePlan = timePlan,
            PhaseDurations = phaseDurations,
            SourceServices = new() { "Intersection Controller Service" },
            DestinationServices = new() { "Traffic Light Controller Service" },
            Metadata = metadata
        };

        await PublishAsync(lightName.ToLower(), msg);

        _logger.LogInformation(
            "[{Intersection}] CONTROL published → {LightName} | Mode={Mode}, Plan={Plan}, Phases=[G:{G},Y:{Y},R:{R}]",
            _intersection.Name,
            lightName,
            operationalMode,
            timePlan,
            phaseDurations.GetValueOrDefault("Green"),
            phaseDurations.GetValueOrDefault("Yellow"),
            phaseDurations.GetValueOrDefault("Red"));
    }

    // ============================================================
    // Internal publish logic
    // ============================================================
    private async Task PublishAsync(string lightKey, TrafficLightControlMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{light}", lightKey);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
