using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TrafficMessages.Light;
using TrafficLightControlService.Services;

namespace TrafficLightControlService.Consumers
{
    public class TrafficLightControlConsumer : IConsumer<TrafficLightControl>
    {
        private readonly ITrafficLightWorker _worker;
        private readonly ILogger<TrafficLightControlConsumer> _logger;

        public TrafficLightControlConsumer(ITrafficLightWorker worker, ILogger<TrafficLightControlConsumer> logger)
        {
            _worker = worker;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TrafficLightControl> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "[CONTROL RECEIVED] Intersection {IntersectionId} pattern {Pattern} for {Duration}s triggered by {TriggeredBy}",
                msg.IntersectionId, msg.ControlPattern, msg.DurationSeconds, msg.TriggeredBy);

            await _worker.ApplyControlAsync(msg);
        }
    }
}
