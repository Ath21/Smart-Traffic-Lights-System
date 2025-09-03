using System;
using IntersectionControllerStore.Publishers.LightPub;
using IntersectionControllerStore.Publishers.LogPub;
using IntersectionControllerStore.Publishers.PriorityPub;

namespace IntersectionControllerStore.Business.Priority;

public class PriorityManager : IPriorityManager
{
    private readonly IPriorityPublisher _priorityPublisher;
    private readonly ITrafficLightControlPublisher _lightControlPublisher;
    private readonly ITrafficLogPublisher _logPublisher;
    private readonly ILogger<PriorityManager> _logger;

    private const string ServiceTag = "[" + nameof(PriorityManager) + "]";

    public PriorityManager(
        IPriorityPublisher priorityPublisher,
        ITrafficLightControlPublisher lightControlPublisher,
        ITrafficLogPublisher logPublisher,
        ILogger<PriorityManager> logger)
    {
        _priorityPublisher = priorityPublisher;
        _lightControlPublisher = lightControlPublisher;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task ProcessVehicleCountAsync(Guid intersectionId, int count, float avgSpeed, DateTime timestamp)
    {
        _logger.LogInformation("{Tag} VehicleCount: {Count} at {IntersectionId}, AvgSpeed={AvgSpeed}", ServiceTag, count, intersectionId, avgSpeed);
        // Example: maybe log congestion events, but not publish priority directly
        await _logPublisher.PublishAuditAsync(nameof(PriorityManager), "VehicleCountProcessed", $"Count={count}, AvgSpeed={avgSpeed}");
    }

    public async Task ProcessEmergencyVehicleAsync(Guid intersectionId, Guid detectionId, bool detected, DateTime timestamp)
    {
        _logger.LogInformation("{Tag} EmergencyVehicle detected={Detected} at {IntersectionId}", ServiceTag, detected, intersectionId);
        if (detected)
        {
            await _priorityPublisher.PublishPriorityAsync(intersectionId, "emergency", detectionId, "Emergency vehicle detected");
        }
    }

    public async Task ProcessPublicTransportAsync(Guid intersectionId, Guid detectionId, string? routeId, DateTime timestamp)
    {
        _logger.LogInformation("{Tag} PublicTransport request at {IntersectionId} Route={RouteId}", ServiceTag, intersectionId, routeId);
        await _priorityPublisher.PublishPriorityAsync(intersectionId, "public_transport", detectionId, $"Route {routeId} priority request");
    }

    public async Task ProcessPedestrianAsync(Guid intersectionId, Guid detectionId, int count, DateTime timestamp)
    {
        _logger.LogInformation("{Tag} Pedestrian request at {IntersectionId} Count={Count}", ServiceTag, intersectionId, count);
        await _priorityPublisher.PublishPriorityAsync(intersectionId, "pedestrian", detectionId, $"Pedestrian request: {count} people waiting");
    }

    public async Task ProcessCyclistAsync(Guid intersectionId, Guid detectionId, int count, DateTime timestamp)
    {
        _logger.LogInformation("{Tag} Cyclist request at {IntersectionId} Count={Count}", ServiceTag, intersectionId, count);
        await _priorityPublisher.PublishPriorityAsync(intersectionId, "cyclist", detectionId, $"Cyclist request: {count} bikes waiting");
    }

    public async Task ProcessIncidentAsync(Guid intersectionId, Guid detectionId, string description, DateTime timestamp)
    {
        _logger.LogWarning("{Tag} Incident detected at {IntersectionId}: {Description}", ServiceTag, intersectionId, description);
        await _priorityPublisher.PublishPriorityAsync(intersectionId, "incident", detectionId, description);
        await _logPublisher.PublishErrorAsync(nameof(PriorityManager), "IncidentDetected", description, new { intersectionId, detectionId });
    }
}
