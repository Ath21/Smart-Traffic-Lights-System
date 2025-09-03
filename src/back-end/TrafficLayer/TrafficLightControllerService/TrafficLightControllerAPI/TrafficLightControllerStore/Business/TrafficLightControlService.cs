using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficMessages;
using TrafficLightControllerStore.Repository;
using TrafficLightControllerStore.Models.Dtos;
using TrafficLightControllerStore.Publishers.Logs;

namespace TrafficLightControllerStore.Business;

public class TrafficLightControlService : ITrafficLightControlService
{
    private readonly ITrafficLightRepository _repository;
    private readonly IBus _bus;
    private readonly ITrafficLogPublisher _logPublisher;
    private readonly ILogger<TrafficLightControlService> _logger;
    private readonly string _controlKey;

    private const string ServiceTag = "[" + nameof(TrafficLightControlService) + "]";

    public TrafficLightControlService(
        ITrafficLightRepository repository,
        IBus bus,
        ITrafficLogPublisher logPublisher,
        ILogger<TrafficLightControlService> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _bus = bus;
        _logPublisher = logPublisher;
        _logger = logger;

        _controlKey = configuration["RabbitMQ:RoutingKeys:TrafficLightControl"]
            ?? "traffic.light.control.{intersection_id}.{light_id}";
    }

    /// <summary>
    /// Force state change for a light (manual override).
    /// Updates Redis and publishes control message.
    /// </summary>
    public async Task<TrafficLightDto> ForceStateChangeAsync(Guid intersectionId, Guid lightId, string newState)
    {
        var updatedAt = DateTime.UtcNow;

        try
        {
            // Save state & control event in Redis
            await _repository.SetLightStateAsync(intersectionId, lightId, newState);
            await _repository.SaveControlEventAsync(intersectionId, lightId, newState);

            // Publish to RabbitMQ
            var key = _controlKey
                .Replace("{intersection_id}", intersectionId.ToString())
                .Replace("{light_id}", lightId.ToString());

            var msg = new TrafficLightControlMessage(intersectionId, lightId, newState, updatedAt);
            await _bus.Publish(msg, ctx => ctx.SetRoutingKey(key));

            _logger.LogInformation(
                "{Tag} Manual override applied: Intersection={IntersectionId}, Light={LightId}, State={State}",
                ServiceTag, intersectionId, lightId, newState);

            // New simplified audit log call
            await _logPublisher.PublishAuditAsync(
                "LightStateOverride",
                $"Intersection={intersectionId}, Light={lightId}, NewState={newState}",
                new { updatedAt });

            return new TrafficLightDto
            {
                IntersectionId = intersectionId,
                LightId = lightId,
                State = newState,
                UpdatedAt = updatedAt
            };
        }
        catch (Exception ex)
        {
            // New simplified error log call
            await _logPublisher.PublishErrorAsync(
                "OverrideFailed",
                $"Failed to override light {lightId} in intersection {intersectionId}",
                new { newState, ex });

            throw;
        }
    }

    /// <summary>
    /// Get all light states of an intersection.
    /// </summary>
    public async Task<IEnumerable<TrafficLightDto>> GetCurrentStatesAsync(Guid intersectionId)
    {
        var lights = new List<TrafficLightDto>();

        var lastControl = await _repository.GetLastControlEventAsync(intersectionId, Guid.Empty);
        if (lastControl != null)
        {
            lights.Add(new TrafficLightDto
            {
                IntersectionId = intersectionId,
                LightId = Guid.Empty, // until registry exists
                State = lastControl,
                UpdatedAt = DateTime.UtcNow
            });
        }

        return lights;
    }

    public async Task<IEnumerable<ControlEventDto>> GetLastControlEventsAsync(Guid intersectionId)
    {
        var events = await _repository.GetControlEventsAsync(intersectionId);

        return events.Select(e => new ControlEventDto
        {
            IntersectionId = intersectionId,
            LightId = e.LightId,
            Command = e.Command,
            Timestamp = e.Timestamp
        });
    }
}
