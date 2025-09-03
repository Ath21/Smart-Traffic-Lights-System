using System;
using IntersectionControllerStore.Business.CommandLog;
using IntersectionControllerStore.Business.TrafficLight;
using IntersectionControllerStore.Publishers.LogPub;

namespace IntersectionControllerStore.Business.Coordinator;

public class TrafficLightCoordinatorService : ITrafficLightCoordinatorService
{
    private readonly ITrafficLightService _lightService;
    private readonly ICommandLogService _logService;
    private readonly ITrafficLogPublisher _publisher;
    private readonly ILogger<TrafficLightCoordinatorService> _logger;

    private const string ServiceTag = "[" + nameof(TrafficLightCoordinatorService) + "]";

    public TrafficLightCoordinatorService(
        ITrafficLightService lightService,
        ICommandLogService logService,
        ITrafficLogPublisher publisher,
        ILogger<TrafficLightCoordinatorService> logger)
    {
        _lightService = lightService;
        _logService = logService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task ApplyUpdateAsync(Guid intersectionId, Guid lightId, string newState, DateTime updatedAt)
    {
        // Update Redis HASH state
        await _lightService.UpdateLightStateAsync(intersectionId, lightId, newState);

        // Push to Redis LIST (command log)
        await _logService.LogCommandAsync(intersectionId, new
        {
            LightId = lightId,
            State = newState,
            UpdatedAt = updatedAt
        });

        // Publish to RabbitMQ logs
        await _publisher.PublishAuditAsync(
            nameof(TrafficLightCoordinatorService),
            "LightStateChanged",
            $"Intersection={intersectionId}, Light={lightId}, State={newState}",
            new { intersectionId, lightId, state = newState, updatedAt }
        );

        _logger.LogInformation(
            "{Tag} Applied update at Intersection {IntersectionId}, Light {LightId} → {State}",
            ServiceTag, intersectionId, lightId, newState);
    }
}
