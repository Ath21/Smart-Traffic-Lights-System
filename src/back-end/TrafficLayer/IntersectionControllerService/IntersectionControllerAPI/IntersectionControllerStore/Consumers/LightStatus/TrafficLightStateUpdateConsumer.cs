using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficMessages.Light;

namespace TrafficLightControlService.Consumers
{
    public class TrafficLightStateUpdateConsumer : IConsumer<TrafficLightStateUpdate>
    {
        private readonly ILogger<TrafficLightStateUpdateConsumer> _logger;

        public TrafficLightStateUpdateConsumer(ILogger<TrafficLightStateUpdateConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<TrafficLightStateUpdate> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received TrafficLightStateUpdate for Intersection {IntersectionId}: Pattern={Pattern} at {Timestamp}",
                message.IntersectionId,
                message.CurrentPattern,
                message.Timestamp);

            // TODO: Implement logic to update traffic light hardware or internal state accordingly

            return Task.CompletedTask;
        }
    }
}
