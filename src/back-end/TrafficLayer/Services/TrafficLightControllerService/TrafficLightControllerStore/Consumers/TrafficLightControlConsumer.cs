using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficLightCacheData.Entities;
using TrafficLightCacheData.Repositories.Light;
using TrafficMessages;
using TrafficLightControllerStore.Models.Dtos;

namespace TrafficLightControllerStore.Consumers;

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
            "[TrafficLightControlConsumer] Received CONTROL → Intersection={Intersection}, Light={Light}, State={State}, Duration={Duration}, Reason={Reason}, IssuedAt={IssuedAt}",
            msg.Intersection, msg.Light, msg.NewState, msg.Duration, msg.Reason, msg.IssuedAt);

        // 1. Update Redis hash with the new light state
        await _repository.SetLightStateAsync(msg.Intersection, msg.Light, msg.NewState);

        // 2. If override info is provided, persist it too
        if (msg.Duration.HasValue)
        {
            var expiresAt = msg.IssuedAt.AddSeconds(msg.Duration.Value);
            await _repository.SetOverrideAsync(
                msg.Intersection,
                msg.Light,
                msg.NewState,
                msg.Duration.Value,
                msg.Reason,
                expiresAt
            );
        }

        // 3. Map DTO → Entity and save full traffic light state
        var dto = new TrafficLightDto
        {
            Intersection = msg.Intersection,
            Light = msg.Light,
            State = msg.NewState,
            UpdatedAt = DateTime.UtcNow
        };

        var entity = _mapper.Map<TrafficLight>(dto);
        await _repository.SaveAsync(entity);

        _logger.LogInformation(
            "[TrafficLightControlConsumer] Applied CONTROL → {Intersection}-{Light} set to {State}",
            msg.Intersection, msg.Light, msg.NewState);
    }
}
