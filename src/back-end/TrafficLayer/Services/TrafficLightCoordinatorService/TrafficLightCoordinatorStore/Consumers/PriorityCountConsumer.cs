using System;
using MassTransit;
using Messages.Traffic;
using TrafficLightCoordinatorStore.Business;

namespace TrafficLightCoordinatorStore.Consumers;

public class PriorityCountConsumer : IConsumer<PriorityCountMessage>
{
    private readonly ICoordinatorBusiness _decisionService;
    private readonly ILogger<PriorityCountConsumer> _logger;

    public PriorityCountConsumer(ICoordinatorBusiness decisionService, ILogger<PriorityCountConsumer> logger)
    {
        _decisionService = decisionService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PriorityCountMessage> context)
    {
        _logger.LogInformation("[CONSUMER] PriorityCountMessage received ({Type})", context.Message.CountType);
        await _decisionService.HandlePriorityCountAsync(context.Message);
    }
}
