using MassTransit;
using Messages.Traffic;

namespace TrafficLightCoordinator.Publishers.Update;

public class TrafficLightUpdatePublisher : ITrafficLightUpdatePublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightUpdatePublisher> _logger;
    private readonly IntersectionContext _context;
    private readonly string _routingPattern;

    public TrafficLightUpdatePublisher(
        IConfiguration config,
        ILogger<TrafficLightUpdatePublisher> logger,
        IBus bus,
        IntersectionContext context)
    {
        _bus = bus;
        _logger = logger;
        _context = context;

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:LightUpdate"]
                          ?? "traffic.light.update.{intersection}";
    }

    // ============================================================
    // Publish full update for one intersection
    // ============================================================
    public async Task PublishUpdateAsync(
        string intersectionName,
        bool isOperational,
        string currentMode,
        string timePlan,
        Dictionary<string, int> phaseDurations,
        int cycleDurationSec,
        int globalOffsetSec,
        Dictionary<int, int> lightOffsets,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        var intersection = _context.GetByName(intersectionName)
            ?? throw new InvalidOperationException($"Intersection '{intersectionName}' not found in context.");

        var msg = new TrafficLightUpdateMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            IntersectionId = intersection.IntersectionId,
            IntersectionName = intersection.Name,
            SourceServices = new() { "Traffic Light Coordinator Service" },
            DestinationServices = new() { "Intersection Controller Service" },
            IsOperational = isOperational,
            CurrentMode = currentMode,
            TimePlan = timePlan,
            PhaseDurations = phaseDurations,
            CycleDurationSec = cycleDurationSec,
            GlobalOffsetSec = globalOffsetSec,
            LightOffsets = lightOffsets,
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };

        await PublishAsync(intersection.Name, msg);

        _logger.LogInformation(
            "[{Intersection}] UPDATE published (Mode={Mode}, Plan={Plan}, Cycle={Cycle}s, Offset={Offset}s)",
            intersection.Name, currentMode, timePlan, cycleDurationSec, globalOffsetSec);
    }

    // ============================================================
    // Internal publish logic
    // ============================================================
    private async Task PublishAsync(string intersectionName, TrafficLightUpdateMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;

        var routingKey = _routingPattern
            .Replace("{intersection}", intersectionName.ToLower().Replace(' ', '-'));

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
