using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficMessages;
using TrafficLightControlStore.Repository;
using TrafficLightControlStore.Models.Dtos;

namespace TrafficLightControlStore.Business;

public class TrafficLightControlService : ITrafficLightControlService
{
    private readonly ITrafficLightRepository _repository;
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightControlService> _logger;
    private readonly string _controlKey;

    private const string ServiceTag = "[" + "TrafficLightControlService" + "]";

    public TrafficLightControlService(
        ITrafficLightRepository repository,
        IBus bus,
        ILogger<TrafficLightControlService> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _bus = bus;
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
        // Save state & log event in Redis
        await _repository.SetLightStateAsync(intersectionId, lightId, newState);
        await _repository.SaveControlEventAsync(intersectionId, lightId, newState);

        // Publish to RabbitMQ
        var key = _controlKey
            .Replace("{intersection_id}", intersectionId.ToString())
            .Replace("{light_id}", lightId.ToString());

        var msg = new TrafficLightControlMessage(intersectionId, lightId, newState, DateTime.UtcNow);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(key));

        _logger.LogInformation(
            "{Tag} Manual override applied: Intersection={IntersectionId}, Light={LightId}, State={State}",
            ServiceTag, intersectionId, lightId, newState);

        // Map to DTO for response
        return new TrafficLightDto
        {
            IntersectionId = intersectionId,
            LightId = lightId,
            State = newState,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get all light states of an intersection.
    /// </summary>
    public async Task<IEnumerable<TrafficLightDto>> GetCurrentStatesAsync(Guid intersectionId)
    {
        // In real DB, we’d fetch all. Here, we just simulate single light retrieval
        // TODO: if lights list is known, loop over IDs.
        // For now, empty list (placeholder until lights registry exists).
        return Enumerable.Empty<TrafficLightDto>();
    }
}
