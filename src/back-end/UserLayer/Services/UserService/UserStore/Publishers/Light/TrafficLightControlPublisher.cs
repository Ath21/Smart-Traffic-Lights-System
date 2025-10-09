using MassTransit;
using Messages.Traffic;

namespace UserStore.Publishers.Light;

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

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:LightControl"]
                          ?? "traffic.light.control.{intersection}.{light}";
    }

    public async Task PublishControlAsync(
        int lightId,
        string lightName,
        string mode,
        string timePlan,
        Dictionary<string, int> phaseDurations,
        string operationalMode,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        var msg = new TrafficLightControlMessage
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            LightId = lightId,
            LightName = lightName,
            Mode = mode,
            TimePlan = timePlan,
            PhaseDurations = phaseDurations,
            OperationalMode = operationalMode,
            SourceServices = new() { "User Service" },
            DestinationServices = new() { "Traffic Light Controller Service" },
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{light}", lightName.ToLower().Replace(' ', '-'));

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[{Intersection}] CONTROL message sent → {LightName} (Mode={Mode}, Plan={Plan})",
            _intersection.Name, lightName, mode, timePlan);
    }
}
