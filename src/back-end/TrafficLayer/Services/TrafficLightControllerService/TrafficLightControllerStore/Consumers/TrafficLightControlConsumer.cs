using MassTransit;
using Messages.Traffic;
using Messages.Traffic.Light;
using TrafficLightCacheData.Repositories;
using TrafficLightControllerStore.Publishers.Logs;

namespace TrafficLightControllerStore.Consumers;

public class TrafficLightControlConsumer : IConsumer<TrafficLightControlMessage>
{
    private readonly ITrafficLightCacheRepository _cache;
    private readonly ILogger<TrafficLightControlConsumer> _logger;
    private readonly ITrafficLightLogPublisher _logPublisher;

    public TrafficLightControlConsumer(
        ITrafficLightCacheRepository cache,
        ITrafficLightLogPublisher logPublisher,
        ILogger<TrafficLightControlConsumer> logger)
    {
        _cache = cache;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficLightControlMessage> context)
    {
        var msg = context.Message;

        try
        {
            // =================================
            // Update core state
            // =================================
            await _cache.SetStateAsync(msg.IntersectionId, msg.LightId, msg.OperationalMode ?? "Unknown");
            await _cache.SetModeAsync(msg.IntersectionId, msg.LightId, msg.Mode ?? "Unknown");
            await _cache.SetPriorityAsync(msg.IntersectionId, msg.LightId, msg.PriorityLevel);
            await _cache.SetLastUpdateAsync(msg.IntersectionId, msg.LightId, msg.LastUpdate);

            // =================================
            // Update phase & cycle
            // =================================
            if (msg.PhaseDurations != null)
                await _cache.SetCachedPhasesAsync(msg.IntersectionId, msg.LightId, msg.PhaseDurations);

            await _cache.SetCurrentPhaseAsync(msg.IntersectionId, msg.LightId, msg.CurrentPhase ?? "Green");
            await _cache.SetRemainingTimeAsync(msg.IntersectionId, msg.LightId, msg.RemainingTimeSec);
            await _cache.SetCycleDurationAsync(msg.IntersectionId, msg.LightId, msg.CycleDurationSec);
            await _cache.SetLocalOffsetAsync(msg.IntersectionId, msg.LightId, msg.LocalOffsetSec);
            await _cache.SetCycleProgressAsync(msg.IntersectionId, msg.LightId, msg.CycleProgressSec);

            // =================================
            // Update failover
            // =================================
            await _cache.SetFailoverAsync(msg.IntersectionId, msg.LightId, msg.IsFailoverActive);

            // =================================
            // Diagnostics / heartbeat
            // =================================
            await _cache.SetHeartbeatAsync(msg.IntersectionId, msg.LightId);
            await _cache.SetCoordinatorSyncAsync(msg.IntersectionId);

            // =================================
            // Logging
            // =================================
            await _logPublisher.PublishAuditAsync(
                "TrafficLightControlApplied",
                $"Traffic light control updated for intersection {msg.IntersectionName}",
                new()
                {
                    ["LightName"] = msg.LightName,
                    ["Mode"] = msg.Mode,
                    ["OperationalMode"] = msg.OperationalMode,
                    ["PriorityLevel"] = msg.PriorityLevel,
                    ["CurrentPhase"] = msg.CurrentPhase,
                    ["RemainingTimeSec"] = msg.RemainingTimeSec
                },
                context.CorrelationId?.ToString()
            );

            _logger.LogInformation(
                "Traffic light control applied: {Intersection}-{Light} Mode={Mode}, Phase={Phase}",
                msg.IntersectionName,
                msg.LightName,
                msg.Mode,
                msg.CurrentPhase
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply traffic light control for intersection {Intersection}", msg.IntersectionName);
            await _logPublisher.PublishErrorAsync(
                "TrafficLightControlConsumer",
                $"Failed to apply traffic light control for {msg.IntersectionName}",
                ex,
                new() { ["LightName"] = msg.LightName },
                context.CorrelationId?.ToString()
            );
        }
    }
}