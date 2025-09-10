using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficLightCacheData.Entities;
using TrafficLightCacheData.Repositories.Light;
using TrafficMessages;
using TrafficLightControllerStore.Models.Dtos;

namespace TrafficLightControllerStore.Consumers;

/// <summary>
/// Consumes control commands for specific traffic lights and applies them to Redis.
/// </summary>
public class TrafficLightControlConsumer : IConsumer<TrafficLightControlMessage>
{
    private readonly ILogger<TrafficLightControlConsumer> _logger;
    private readonly ITrafficLightRepository _repository;
    private readonly IMapper _mapper;

    public TrafficLightControlConsumer(
        ILogger<TrafficLightControlConsumer> logger,
        ITrafficLightRepository repository,
        IMapper mapper)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<TrafficLightControlMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[TrafficLightControlConsumer] Received CONTROL → Intersection={IntersectionId}, Light={LightId}, NewState={State}, IssuedAt={IssuedAt}",
            msg.IntersectionId, msg.LightId, msg.NewState, msg.IssuedAt);

        // 1. Update Redis hash with the new light state
        await _repository.SetLightStateAsync(msg.IntersectionId, msg.LightId, msg.NewState);

        // 2. Map DTO → Entity and save full traffic light state
        var dto = new TrafficLightDto
        {
            IntersectionId = msg.IntersectionId,
            LightId = msg.LightId,
            State = msg.NewState,
            UpdatedAt = DateTime.UtcNow
        };

        var entity = _mapper.Map<TrafficLight>(dto);
        await _repository.SaveAsync(entity);

        _logger.LogInformation(
            "[TrafficLightControlConsumer] Applied CONTROL → {IntersectionId}-{LightId} set to {State}",
            msg.IntersectionId, msg.LightId, msg.NewState);
    }
}
