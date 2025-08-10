using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages.Light;

namespace IntersectionControlStore.Publishers.LightPub
{
    public class TrafficLightControlPublisher : ITrafficLightControlPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<TrafficLightControlPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _trafficControlExchange;
        private readonly string _trafficLightControlRoutingKeyBase;

        public TrafficLightControlPublisher(
            IBus bus,
            ILogger<TrafficLightControlPublisher> logger,
            IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _trafficControlExchange = _configuration["RabbitMQ:Exchange:TrafficControlExchange"] ?? "traffic.control.exchange";
            _trafficLightControlRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:TrafficLightControl"] ?? "traffic.intersection_control.light.*";
        }

        public async Task PublishTrafficLightControlAsync(TrafficLightControl controlMessage)
        {
            var routingKey = BuildRoutingKey(_trafficLightControlRoutingKeyBase, controlMessage.IntersectionId);

            _logger.LogInformation("[TRAFFIC LIGHT CONTROL] Publishing control command to '{RoutingKey}' on exchange '{Exchange}'", routingKey, _trafficControlExchange);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_trafficControlExchange}"));
            await sendEndpoint.Send(controlMessage, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[TRAFFIC LIGHT CONTROL] Published control command for intersection {IntersectionId} with pattern '{Pattern}' for {Duration}s triggered by {TriggeredBy}",
                controlMessage.IntersectionId, controlMessage.ControlPattern, controlMessage.DurationSeconds, controlMessage.TriggeredBy);
        }

        private string BuildRoutingKey(string routingKeyBase, string intersectionId)
        {
            var baseKey = routingKeyBase.TrimEnd('*').TrimEnd('.');
            return $"{baseKey}.{intersectionId}";
        }
    }
}
