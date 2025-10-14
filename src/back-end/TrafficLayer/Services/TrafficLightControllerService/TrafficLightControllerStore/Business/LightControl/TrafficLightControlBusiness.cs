using Messages.Traffic;
using TrafficLightCacheData.Repositories;
using TrafficLightControllerStore.Publishers.Logs;

namespace TrafficLightControllerStore.Business.LightControl;

public class TrafficLightControlBusiness
{
    private readonly ITrafficLightCacheRepository _cache;
    private readonly ITrafficLightLogPublisher _logPublisher;
    private readonly ILogger<TrafficLightControlBusiness> _logger;

    public TrafficLightControlBusiness(
        ITrafficLightCacheRepository cache,
        ITrafficLightLogPublisher logPublisher,
        ILogger<TrafficLightControlBusiness> logger)
    {
        _cache = cache;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task ApplyControlMessageAsync(TrafficLightControlMessage msg)
    {
        _logger.LogInformation(
            "[BUSINESS][CONTROL][{Intersection}/{Light}] Applying control message (Mode={Mode}, Phase={Phase}, Remaining={Remaining}s)",
            msg.IntersectionName,
            msg.LightName,
            msg.Mode ?? msg.OperationalMode ?? "Unknown",
            msg.CurrentPhase ?? "N/A",
            msg.RemainingTimeSec);

        // ============================================================
        // Update Redis cache
        // ============================================================
        await _cache.SetModeAsync(msg.IntersectionId, msg.LightId, msg.Mode ?? msg.OperationalMode ?? "Unknown");
        await _cache.SetStateAsync(msg.IntersectionId, msg.LightId, msg.CurrentPhase ?? "Unknown");
        await _cache.SetDurationAsync(msg.IntersectionId, msg.LightId, msg.RemainingTimeSec);
        await _cache.SetLastUpdateAsync(msg.IntersectionId, msg.LightId, msg.LastUpdate);

        await _cache.SetCycleDurationAsync(msg.IntersectionId, msg.LightId, msg.CycleDurationSec);
        await _cache.SetOffsetAsync(msg.IntersectionId, msg.LightId, msg.LocalOffsetSec);
        await _cache.SetPriorityAsync(msg.IntersectionId, msg.LightId, msg.PriorityLevel);
        await _cache.SetFailoverAsync(msg.IntersectionId, msg.LightId, msg.IsFailoverActive);

        await _cache.SetRemainingTimeAsync(msg.IntersectionId, msg.LightId, msg.RemainingTimeSec);
        await _cache.SetCurrentPhaseAsync(msg.IntersectionId, msg.LightId, msg.CurrentPhase ?? "Unknown");
        await _cache.SetLocalOffsetAsync(msg.IntersectionId, msg.LightId, msg.LocalOffsetSec);

        // ============================================================
        // Log audit event
        // ============================================================
        var metadata = new Dictionary<string, string>
        {
            ["intersection"] = msg.IntersectionName ?? "Unknown",
            ["light_name"] = msg.LightName ?? "Unknown",
            ["mode"] = msg.Mode ?? msg.OperationalMode ?? "Unknown",
            ["phase"] = msg.CurrentPhase ?? "Unknown",
            ["remaining_time_sec"] = msg.RemainingTimeSec.ToString(),
            ["cycle_duration_sec"] = msg.CycleDurationSec.ToString(),
            ["local_offset_sec"] = msg.LocalOffsetSec.ToString(),
            ["priority_level"] = msg.PriorityLevel.ToString(),
            ["failover_active"] = msg.IsFailoverActive.ToString(),
            ["timestamp"] = msg.LastUpdate.ToString("o")
        };

        await _logPublisher.PublishAuditAsync(
            action: "TRAFFIC_LIGHT_CONTROL_APPLIED",
            message: $"Light {msg.LightName} set to phase {msg.CurrentPhase} ({msg.RemainingTimeSec}s remaining)",
            metadata: metadata,
            correlationId: msg.CorrelationId);

        _logger.LogInformation(
            "[BUSINESS][CONTROL][{Intersection}/{Light}] Cache updated successfully",
            msg.IntersectionName,
            msg.LightName);
    }
}