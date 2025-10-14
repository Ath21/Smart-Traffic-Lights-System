using Microsoft.Extensions.Logging;
using IntersectionControllerStore.Publishers;
using Messages.Traffic;
using IntersectionControllerStore.Publishers.LightControl;
using IntersectionControllerStore.Publishers.Logs;

namespace IntersectionControllerStore.Business.Failover;

public class FailoverBusiness : IFailoverBusiness
{
    private readonly TrafficLightControlPublisher _controlPublisher;
    private readonly IntersectionLogPublisher _logPublisher;
    private readonly ILogger<FailoverBusiness> _logger;

    public FailoverBusiness(
        TrafficLightControlPublisher controlPublisher,
        IntersectionLogPublisher logPublisher,
        ILogger<FailoverBusiness> logger)
    {
        _controlPublisher = controlPublisher;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task TriggerFailoverAsync(int intersectionId, string intersectionName, List<int> lightIds, string reason)
    {
        _logger.LogWarning("[FAILOVER][{Intersection}] Triggering failover: {Reason}", intersectionName, reason);

        foreach (var lightId in lightIds)
        {
            var msg = new TrafficLightControlMessage
            {
                CorrelationId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                SourceLayer = "Control Layer",
                DestinationLayer = new() { "Edge Layer" },
                SourceService = "Intersection Controller Service",
                DestinationServices = new() { "Traffic Light Controller Service" },
                IntersectionId = intersectionId,
                IntersectionName = intersectionName,
                LightId = lightId,
                LightName = $"{intersectionName.ToLower().Replace(' ', '-')}-{lightId}",
                Mode = "Failover",
                OperationalMode = "Flashing",
                PhaseDurations = new() { ["Green"] = 0, ["Yellow"] = 0, ["Red"] = 0 },
                CurrentPhase = "FlashingYellow",
                RemainingTimeSec = 0,
                CycleDurationSec = 0,
                LocalOffsetSec = 0,
                CycleProgressSec = 0,
                PriorityLevel = 3,
                IsFailoverActive = true
            };

            await _controlPublisher.PublishControlAsync(msg);
        }

        await _logPublisher.PublishFailoverAsync(
            "FailoverTriggered",
            $"Failover activated for {intersectionName}",
            new()
            {
                ["intersection_id"] = intersectionId.ToString(),
                ["lights_affected"] = string.Join(",", lightIds),
                ["reason"] = reason
            });

        _logger.LogWarning("[FAILOVER][{Intersection}] Failover commands sent for {Count} lights",
            intersectionName, lightIds.Count);
    }
}
