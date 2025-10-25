using System;
using Messages.Traffic.Light;
using Microsoft.EntityFrameworkCore;
using TrafficLightCoordinatorStore.Publishers.Logs;
using TrafficLightCoordinatorStore.Publishers.Schedule;
using TrafficLightData;

namespace TrafficLightCoordinatorStore.Business.Operator;

public class TrafficOperatorBusiness
{
    private readonly TrafficLightDbContext _db;
    private readonly ITrafficLightSchedulePublisher _schedulePublisher;
    private readonly ICoordinatorLogPublisher _log;
    private readonly ILogger<TrafficOperatorBusiness> _logger;

    public TrafficOperatorBusiness(
        TrafficLightDbContext db,
        ITrafficLightSchedulePublisher schedulePublisher,
        ICoordinatorLogPublisher log,
        ILogger<TrafficOperatorBusiness> logger)
    {
        _db = db;
        _schedulePublisher = schedulePublisher;
        _log = log;
        _logger = logger;
    }

    public async Task ApplyModeAsync(int intersectionId, string mode)
    {
        var intersection = await _db.Intersections
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.IntersectionId == intersectionId);

        if (intersection == null)
        {
            _logger.LogWarning("Intersection {Id} not found", intersectionId);
            throw new KeyNotFoundException($"Intersection {intersectionId} not found");
        }

        var config = await _db.TrafficConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Mode == mode)
            ?? throw new KeyNotFoundException($"Mode '{mode}' not found in configurations");

        var schedule = new TrafficLightScheduleMessage
        {
            IntersectionId = intersection.IntersectionId,
            IntersectionName = intersection.Name,
            CurrentMode = config.Mode,
            IsOperational = true,
            CycleDurationSec = config.CycleDurationSec,
            GlobalOffsetSec = config.GlobalOffsetSec,
            PhaseDurations = System.Text.Json.JsonSerializer
                                .Deserialize<Dictionary<string, int>>(config.PhaseDurationsJson) ?? new(),
            Purpose = config.Purpose,
            LastUpdate = DateTime.UtcNow
        };

        await _schedulePublisher.PublishUpdateAsync(
            schedule.IntersectionId,
            schedule.IntersectionName!,
            schedule.IsOperational,
            schedule.CurrentMode,
            schedule.PhaseDurations,
            schedule.CycleDurationSec,
            schedule.GlobalOffsetSec,
            schedule.Purpose
        );

        await _log.PublishAuditAsync(
            "TrafficOperator",
            $"Manual mode applied for {intersection.Name}: {config.Mode}",
            "[TRAFFIC-OPERATOR]",
            "manual-mode",
            new Dictionary<string, object>
            {
                ["IntersectionId"] = intersection.IntersectionId,
                ["Mode"] = config.Mode
            });

        _logger.LogInformation("[TRAFFIC-OPERATOR] Applied mode {Mode} to intersection {Intersection}", config.Mode, intersection.Name);
    }
}
