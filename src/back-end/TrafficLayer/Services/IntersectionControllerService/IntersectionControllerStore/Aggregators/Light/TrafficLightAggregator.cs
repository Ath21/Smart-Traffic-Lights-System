using System;
using IntersectionControllerStore.Domain;
using IntersectionControllerStore.Publishers.Light;
using IntersectionControllerStore.Publishers.Logs;
using Messages.Traffic.Light;
using Microsoft.Extensions.Logging;

namespace IntersectionControllerStore.Aggregators.Light;

public class TrafficLightAggregator : ITrafficLightAggregator
{
    private readonly ITrafficLightPublisher _lightPublisher;
    private readonly IIntersectionLogPublisher _logPublisher;
    private readonly ILogger<TrafficLightAggregator> _logger;

    public TrafficLightAggregator(
        ITrafficLightPublisher lightPublisher,
        IIntersectionLogPublisher logPublisher,
        ILogger<TrafficLightAggregator> logger)
    {
        _lightPublisher = lightPublisher;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task<TrafficLightControlMessage> BuildLightControlAsync(
        TrafficLightScheduleMessage schedule,
        TrafficLightContext light)
    {
        var control = new TrafficLightControlMessage
        {
            IntersectionId = schedule.IntersectionId,
            IntersectionName = schedule.IntersectionName,
            LightId = light.Id,
            LightName = light.Name,
            Mode = schedule.CurrentMode,
            OperationalMode = schedule.IsOperational ? "Normal" : "Off",
            PhaseDurations = schedule.PhaseDurations,
            CycleDurationSec = schedule.CycleDurationSec,
            LocalOffsetSec = schedule.GlobalOffsetSec,
            CurrentPhase = "Green",
            RemainingTimeSec = schedule.PhaseDurations.ContainsKey("Green") ? schedule.PhaseDurations["Green"] : 0,
            PriorityLevel = 1,
            IsFailoverActive = false,
            LastUpdate = DateTime.UtcNow
        };

        await _lightPublisher.PublishLightControlAsync(control);

        await _logPublisher.PublishAuditAsync(
            "LightScheduleApplied",
            $"Applied light schedule for light {light.Name} (Intersection {schedule.IntersectionName})",
            category: "system",
            data: new Dictionary<string, object>
            {
                ["LightId"] = light.Id,
                ["Mode"] = schedule.CurrentMode,
                ["CycleDurationSec"] = schedule.CycleDurationSec,
                ["Operational"] = schedule.IsOperational
            });

        _logger.LogInformation(
            "[Intersection {Intersection}] Sent control to light {Light} ({Mode})",
            schedule.IntersectionName,
            light.Name,
            schedule.CurrentMode);

        return control;
    }
}
