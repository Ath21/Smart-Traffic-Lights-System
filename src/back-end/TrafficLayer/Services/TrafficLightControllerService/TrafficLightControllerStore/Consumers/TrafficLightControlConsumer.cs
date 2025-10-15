using MassTransit;
using Messages.Traffic;
using TrafficLightControllerStore.Business;
using TrafficLightControllerStore.Business.LightControl;

namespace TrafficLightControllerStore.Consumers;

public class TrafficLightControlConsumer : IConsumer<TrafficLightControlMessage>
{
    private readonly ITrafficLightControlBusiness _service;
    private readonly ILogger<TrafficLightControlConsumer> _logger;

    public TrafficLightControlConsumer(
        ITrafficLightControlBusiness service,
        ILogger<TrafficLightControlConsumer> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficLightControlMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][CONTROL][{Intersection}/{Light}] Consumed control message (Phase={Phase}, Remaining={Remaining}s)",
            msg.IntersectionName,
            msg.LightName,
            msg.CurrentPhase ?? "N/A",
            msg.RemainingTimeSec);

        await _service.ApplyControlMessageAsync(msg);
    }
}
