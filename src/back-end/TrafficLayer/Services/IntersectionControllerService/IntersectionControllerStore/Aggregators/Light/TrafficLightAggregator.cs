using System;
using IntersectionControllerStore.Publishers.Light;
using IntersectionControllerStore.Publishers.Logs;
using Messages.Traffic.Light;

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

    public async Task<TrafficLightControlMessage> BuildLightControlAsync(TrafficLightScheduleMessage schedule)
    {
        // For each intersection, we build a control model (light controller will interpret it)
        var control = new TrafficLightControlMessage
        {
            IntersectionId = schedule.IntersectionId,
            IntersectionName = schedule.IntersectionName,
            LightId = schedule.IntersectionId, // same as intersection in single-light test setups
            LightName = $"{schedule.IntersectionName}-MainLight",
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

        // Publish to light controllers topic
        await _lightPublisher.PublishLightControlAsync(control);

        // Audit for Coordinator traceability
        await _logPublisher.PublishAuditAsync(
            "LightScheduleApplied",
            $"Applied light schedule for intersection {schedule.IntersectionName}",
            new()
            {
                ["Mode"] = schedule.CurrentMode,
                ["CycleDurationSec"] = schedule.CycleDurationSec,
                ["Operational"] = schedule.IsOperational
            });

        _logger.LogInformation(
            "Light control built and published for intersection {Intersection} ({Mode})",
            schedule.IntersectionName,
            schedule.CurrentMode);

        return control;
    }
}
