using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TrafficMessages.Light;
using TrafficLightControlStore.Business;

namespace TrafficLightControlService.Consumers
{
    public class TrafficLightControlConsumer : IConsumer<TrafficLightControl>
    {
        private readonly ITrafficLightManager _manager;
        private readonly ILogger<TrafficLightControlConsumer> _logger;

        public TrafficLightControlConsumer(ITrafficLightManager manager, ILogger<TrafficLightControlConsumer> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TrafficLightControl> context)
        {
            var msg = context.Message;
            _logger.LogInformation("[CONTROL RECEIVED] {IntersectionId} -> {Pattern} for {Duration}s by {TriggeredBy}",
                msg.IntersectionId, msg.ControlPattern, msg.DurationSeconds, msg.TriggeredBy);

            await _manager.ApplyControlAsync(msg);
        }
    }
}
