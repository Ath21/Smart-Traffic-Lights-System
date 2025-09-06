using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficLightCoordinatorStore.Business;
using TrafficLightCoordinatorStore.Business.Coordination;
using TrafficLightCoordinatorStore.Models.Dtos;
using TrafficMessages;

namespace TrafficLightCoordinatorStore.Consumers;

public class PriorityMessageConsumer : IConsumer<PriorityMessage>
{
    private readonly ILogger<PriorityMessageConsumer> _logger;
    private readonly ICoordinatorService _coordinatorService;

    private const string ServiceTag = "[" + nameof(PriorityMessageConsumer) + "]";

    public PriorityMessageConsumer(
        ILogger<PriorityMessageConsumer> logger,
        ICoordinatorService coordinatorService)
    {
        _logger = logger;
        _coordinatorService = coordinatorService;
    }

    // traffic.priority.*.{intersection_id}
    public async Task Consume(ConsumeContext<PriorityMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received priority {Type} for Intersection {IntersectionId}, Reason={Reason}, Ts={Ts:O}",
            ServiceTag, msg.PriorityType, msg.IntersectionId, msg.Reason, msg.Timestamp);

        // Map message → DTO
        var dto = new PriorityDto
        {
            IntersectionId = msg.IntersectionId,
            PriorityType   = msg.PriorityType,
            Reason         = msg.Reason ?? "N/A",
            AppliedPattern = PatternBuilder.For(msg.PriorityType, active: true),
            AppliedAt      = DateTimeOffset.UtcNow
        };

        await _coordinatorService.HandlePriorityAsync(dto, context.CancellationToken);
    }
}
