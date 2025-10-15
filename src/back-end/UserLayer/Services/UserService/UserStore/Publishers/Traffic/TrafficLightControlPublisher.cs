using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Traffic;

namespace UserStore.Publishers.Traffic;

public class TrafficLightControlPublisher : ITrafficLightControlPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightControlPublisher> _logger;
    private readonly string _routingPattern;

    public TrafficLightControlPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<TrafficLightControlPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        // Pattern: traffic.light.control.{intersection}.{light}
        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:LightControl"]
                          ?? "traffic.light.control.{intersection}.{light}";
    }

    public async Task PublishControlAsync(
        int intersectionId,
        string intersectionName,
        int lightId,
        string lightName,
        string? mode = null,
        string? timePlan = null,
        string? operationalMode = null,
        Dictionary<string, int>? phaseDurations = null,
        string? currentPhase = null,
        int remainingTimeSec = 0,
        int cycleDurationSec = 0,
        int localOffsetSec = 0,
        double cycleProgressSec = 0,
        int priorityLevel = 1,
        bool isFailoverActive = false,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new TrafficLightControlMessage
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "User Layer",
            DestinationLayer = new() { "Traffic Layer" },

            SourceService = "User Service",
            DestinationServices = new() { "Traffic Light Controller Service" },

            IntersectionId = intersectionId,
            IntersectionName = intersectionName,
            LightId = lightId,
            LightName = lightName,
            Mode = mode,
            TimePlan = timePlan,
            OperationalMode = operationalMode,
            PhaseDurations = phaseDurations,
            CurrentPhase = currentPhase,
            RemainingTimeSec = remainingTimeSec,
            CycleDurationSec = cycleDurationSec,
            LocalOffsetSec = localOffsetSec,
            CycleProgressSec = cycleProgressSec,
            PriorityLevel = priorityLevel,
            IsFailoverActive = isFailoverActive,

            Metadata = metadata
        };

        var routingKey = _routingPattern
            .Replace("{intersection}", intersectionName.ToLower().Replace(' ', '-'))
            .Replace("{light}", lightName.ToLower().Replace(' ', '-'));

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "[PUBLISHER][CONTROL][{Intersection}] Sent traffic light control command to {Light} ({Mode}, {OpMode})",
            intersectionName, lightName, mode, operationalMode);
    }
}
