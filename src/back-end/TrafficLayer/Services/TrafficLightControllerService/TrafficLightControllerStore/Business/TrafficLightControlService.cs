using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficMessages;
using TrafficLightControllerStore.Models.Dtos;
using TrafficLightControllerStore.Publishers.Logs;
using TrafficLightCacheData.Repositories.Light;

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
            ?? "traffic.light.control.{intersection}.{light}";
    }

    /// <summary>
    /// Force state change (manual override or direct control).
    /// Saves to Redis, sets override if duration given, and publishes to RabbitMQ.
    /// </summary>
    public async Task<TrafficLightDto> ForceStateChangeAsync(
        string intersection,
        string light,
        string newState,
        int? duration = null,
        string? reason = null)
    {
        var updatedAt = DateTime.UtcNow;
        var expiresAt = duration.HasValue ? updatedAt.AddSeconds(duration.Value) : (DateTime?)null;

        // Save state in Redis
        await _repository.SetLightStateAsync(intersection, light, newState);

        // Save manual override if provided
        if (duration.HasValue)
        {
            await _repository.SetOverrideAsync(intersection, light, newState, duration.Value, reason, expiresAt);
        }

        // Publish to RabbitMQ
        var key = _controlKey
            .Replace("{intersection}", intersection)
            .Replace("{light}", light);

        var msg = new TrafficLightControlMessage(intersection, light, newState, updatedAt, duration, reason);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(key));

        // Audit log
        await _logPublisher.PublishAuditAsync(
            "LightStateOverride",
            $"Intersection={intersection}, Light={light}, NewState={newState}, Duration={duration}, Reason={reason}",
            intersection,
            light,
            new { updatedAt, expiresAt });

        return new TrafficLightDto
        {
            Intersection = intersection,
            Light = light,
            State = newState,
            Duration = duration,
            OverrideReason = reason,
            OverrideExpiresAt = expiresAt,
            UpdatedAt = updatedAt
        };
    }

    /// <summary>
    /// Get all current states of lights at an intersection.
    /// </summary>
    public async Task<IEnumerable<TrafficLightDto>> GetCurrentStatesAsync(string intersection)
    {
        var states = await _repository.GetAllStatesAsync(intersection);
        return states.Select(kvp => new TrafficLightDto
        {
            Intersection = intersection,
            Light = kvp.Key,
            State = kvp.Value,
            UpdatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get last applied control events at an intersection.
    /// </summary>
    public async Task<IEnumerable<ControlEventDto>> GetLastControlEventsAsync(string intersection)
    {
        var events = await _repository.GetAllStatesAsync(intersection);

        return events.Select(e => new ControlEventDto
        {
            Intersection = intersection,
            Light = e.Key,
            Command = e.Value,
            Timestamp = DateTime.UtcNow // could be replaced with Redis-stored timestamp
        });
    }
}
