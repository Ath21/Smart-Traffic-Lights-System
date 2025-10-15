using System;
using System.Text.Json;
using Messages.Traffic;
using Microsoft.EntityFrameworkCore;
using TrafficLightCoordinatorStore.Engine;
using TrafficLightCoordinatorStore.Publishers.Logs;
using TrafficLightCoordinatorStore.Publishers.Schedule;
using TrafficLightData;

namespace TrafficLightCoordinatorStore.Business;

public class CoordinatorBusiness : ICoordinatorBusiness
{
    private readonly ILogger<CoordinatorBusiness> _logger;
    private readonly IDecisionEngine _engine;
    private readonly ITrafficLightSchedulePublisher _schedulePublisher;
    private readonly ICoordinatorLogPublisher _logPublisher;
    private readonly TrafficLightDbContext _db;

    public CoordinatorBusiness(
        ILogger<CoordinatorBusiness> logger,
        IDecisionEngine engine,
        ITrafficLightSchedulePublisher schedulePublisher,
        ICoordinatorLogPublisher logPublisher,
        TrafficLightDbContext db)
    {
        _logger = logger;
        _engine = engine;
        _schedulePublisher = schedulePublisher;
        _logPublisher = logPublisher;
        _db = db;
    }

    public async Task HandlePriorityCountAsync(PriorityCountMessage message)
    {
        string mode = _engine.EvaluatePriorityCount(message);
        await ApplyModeAsync(message.IntersectionId, message.IntersectionName, mode,
            $"Priority count {message.CountType} ({message.TotalCount})");
    }

    public async Task HandlePriorityEventAsync(PriorityEventMessage message)
    {
        string mode = _engine.EvaluatePriorityEvent(message);
        await ApplyModeAsync(message.IntersectionId, message.IntersectionName, mode,
            $"Priority event {message.EventType} ({message.VehicleType})");
    }

    public async Task HandleTrafficAnalyticsAsync(TrafficAnalyticsMessage message)
    {
        string mode = _engine.EvaluateTrafficAnalytics(message);
        await ApplyModeAsync(message.IntersectionId, message.IntersectionName, mode,
            $"Analytics metric {message.MetricType}");
    }

    // ============================================================
    // Apply selected mode for a specific intersection
    // ============================================================
    private async Task ApplyModeAsync(int intersectionId, string? intersectionName, string mode, string reason)
    {
        _logger.LogInformation("[COORDINATOR][APPLY_MODE][{Intersection}] Applying mode {Mode} due to {Reason}",
            intersectionName, mode, reason);

        var config = await _db.TrafficConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Mode == mode)
            ?? await _db.TrafficConfigurations.AsNoTracking().FirstAsync(c => c.Mode == "Standard");

        var phaseDurations = JsonSerializer.Deserialize<Dictionary<string, int>>(config.PhaseDurationsJson) ?? new();

        await _schedulePublisher.PublishUpdateAsync(
            intersectionId: intersectionId,
            intersectionName: intersectionName ?? "Unknown",
            isOperational: true,
            currentMode: config.Mode,
            phaseDurations: phaseDurations,
            cycleDurationSec: config.CycleDurationSec,
            globalOffsetSec: config.GlobalOffsetSec,
            purpose: config.Purpose
        );

        await _logPublisher.PublishAuditAsync(
            action: "MODE_APPLIED",
            message: $"[{intersectionName}] Mode '{config.Mode}' applied ({reason})"
        );
    }
}