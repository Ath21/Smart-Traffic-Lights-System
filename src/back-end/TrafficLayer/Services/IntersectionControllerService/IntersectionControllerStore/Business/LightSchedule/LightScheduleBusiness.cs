using Messages.Traffic;
using Microsoft.Extensions.Logging;
using IntersectionControllerStore.Publishers;
using TrafficLightCacheData.Repositories;
using IntersectionControllerStore.Publishers.LightControl;
using IntersectionControllerStore.Publishers.Logs;

namespace IntersectionControllerStore.Business.LightSchedule;

public class LightScheduleBusiness : ILightScheduleBusiness
{
    private readonly ITrafficLightCacheRepository _cacheRepo;
    private readonly TrafficLightControlPublisher _controlPublisher;
    private readonly IntersectionLogPublisher _logPublisher;
    private readonly ILogger<LightScheduleBusiness> _logger;

    public LightScheduleBusiness(
        ITrafficLightCacheRepository cacheRepo,
        TrafficLightControlPublisher controlPublisher,
        IntersectionLogPublisher logPublisher,
        ILogger<LightScheduleBusiness> logger)
    {
        _cacheRepo = cacheRepo;
        _controlPublisher = controlPublisher;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task ProcessScheduleAsync(TrafficLightScheduleMessage schedule)
    {
        _logger.LogInformation(
            "[SCHEDULE][{Intersection}] Mode={Mode}, Cycle={Cycle}s, Offset={Offset}s, Purpose={Purpose}",
            schedule.IntersectionName, schedule.CurrentMode, schedule.CycleDurationSec,
            schedule.GlobalOffsetSec, schedule.Purpose);

        try
        {
            if (schedule.PhaseDurations == null || schedule.PhaseDurations.Count == 0)
            {
                _logger.LogWarning("[SCHEDULE][{Intersection}] Missing phase durations, ignoring schedule", schedule.IntersectionName);
                return;
            }

            // Cache intersection-level info
            await _cacheRepo.SetCoordinatorSyncAsync(schedule.IntersectionId);
            await _cacheRepo.SetModeAsync(schedule.IntersectionId, 0, schedule.CurrentMode);
            await _cacheRepo.SetCycleDurationAsync(schedule.IntersectionId, 0, schedule.CycleDurationSec);
            await _cacheRepo.SetOffsetAsync(schedule.IntersectionId, 0, schedule.GlobalOffsetSec);
            await _cacheRepo.SetCachedPhasesAsync(schedule.IntersectionId, 0, schedule.PhaseDurations);

            // Determine lights locally
            var lights = await GetLocalLightsAsync(schedule.IntersectionId);
            int offsetStep = lights.Count > 1 ? schedule.GlobalOffsetSec / lights.Count : 0;

            int idx = 0;
            foreach (var lightId in lights)
            {
                int localOffset = offsetStep * idx++;

                await _cacheRepo.SetLocalOffsetAsync(schedule.IntersectionId, lightId, localOffset);
                await _cacheRepo.SetModeAsync(schedule.IntersectionId, lightId, schedule.CurrentMode);
                await _cacheRepo.SetCycleDurationAsync(schedule.IntersectionId, lightId, schedule.CycleDurationSec);
                await _cacheRepo.SetCachedPhasesAsync(schedule.IntersectionId, lightId, schedule.PhaseDurations);
                await _cacheRepo.SetCurrentPhaseAsync(schedule.IntersectionId, lightId, "Green");
                await _cacheRepo.SetRemainingTimeAsync(schedule.IntersectionId, lightId, schedule.PhaseDurations["Green"]);

                var controlMsg = new TrafficLightControlMessage
                {
                    CorrelationId = schedule.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    SourceLayer = "Control Layer",
                    DestinationLayer = new() { "Edge Layer" },
                    SourceService = "Intersection Controller Service",
                    DestinationServices = new() { "Traffic Light Controller Service" },
                    IntersectionId = schedule.IntersectionId,
                    IntersectionName = schedule.IntersectionName,
                    LightId = lightId,
                    LightName = $"{schedule.IntersectionName?.ToLower().Replace(' ', '-')}-{lightId}",
                    Mode = schedule.CurrentMode,
                    OperationalMode = schedule.IsOperational ? "Normal" : "Off",
                    PhaseDurations = schedule.PhaseDurations,
                    CurrentPhase = "Green",
                    RemainingTimeSec = schedule.PhaseDurations["Green"],
                    CycleDurationSec = schedule.CycleDurationSec,
                    LocalOffsetSec = localOffset,
                    CycleProgressSec = 0,
                    PriorityLevel = 1,
                    IsFailoverActive = false
                };

                await _controlPublisher.PublishControlAsync(controlMsg);
                _logger.LogInformation("[CONTROL][{Intersection}] Light={LightId} Offset={Offset}s Mode={Mode}",
                    schedule.IntersectionName, lightId, localOffset, schedule.CurrentMode);
            }

            await _logPublisher.PublishAuditAsync(
                "ScheduleApplied",
                $"Applied schedule for {schedule.IntersectionName} ({schedule.CurrentMode})",
                new()
                {
                    ["intersection_id"] = schedule.IntersectionId.ToString(),
                    ["mode"] = schedule.CurrentMode,
                    ["cycle_duration"] = schedule.CycleDurationSec.ToString(),
                    ["global_offset"] = schedule.GlobalOffsetSec.ToString()
                },
                schedule.CorrelationId);
        }
        catch (Exception ex)
        {
            await _logPublisher.PublishErrorAsync(
                "ScheduleError",
                $"Failed to apply schedule for {schedule.IntersectionName}",
                ex,
                new() { ["intersection_id"] = schedule.IntersectionId.ToString() },
                schedule.CorrelationId);
            throw;
        }
    }

    private Task<List<int>> GetLocalLightsAsync(int intersectionId)
    {
        var lights = intersectionId switch
        {
            1 => new List<int> { 101, 102, 103 },
            2 => new List<int> { 201, 202, 203 },
            3 => new List<int> { 301, 302, 303 },
            4 => new List<int> { 401, 402 },
            5 => new List<int> { 501, 502 },
            _ => new List<int> { 999 }
        };
        return Task.FromResult(lights);
    }
}
