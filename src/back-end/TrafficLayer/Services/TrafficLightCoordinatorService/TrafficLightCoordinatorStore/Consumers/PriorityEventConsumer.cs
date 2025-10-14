using System;
using MassTransit;
using Messages.Traffic;
using TrafficLightCoordinatorStore.Business;

namespace TrafficLightCoordinatorStore.Consumers;

public class PriorityEventConsumer : IConsumer<PriorityEventMessage>
{
    private readonly ICoordinatorBusiness _decisionService;
    private readonly ILogger<PriorityEventConsumer> _logger;

    public PriorityEventConsumer(ICoordinatorBusiness decisionService, ILogger<PriorityEventConsumer> logger)
    {
        _decisionService = decisionService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PriorityEventMessage> context)
    {
        _logger.LogInformation("[CONSUMER] PriorityEventMessage received ({Event})", context.Message.EventType);
        await _decisionService.HandlePriorityEventAsync(context.Message);
    }
}
