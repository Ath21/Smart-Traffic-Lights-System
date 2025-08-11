using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages.Light;
using TrafficMessages.Logs;  // Assume you have these message contracts

namespace IntersectionControlStore.Publishers.LightPub
{
    public class TrafficLightControlPublisher : ITrafficLightControlPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<TrafficLightControlPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _trafficControlExchange;
        private readonly string _trafficLightControlRoutingKeyBase;

        private readonly string _logExchange;
        private readonly string _auditLogRoutingKey;
        private readonly string _errorLogRoutingKey;

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

            _logExchange = _configuration["RabbitMQ:Exchange:LogStoreExchange"] ?? "log.store.exchange";
            _auditLogRoutingKey = "traffic.logs.audit";
            _errorLogRoutingKey = "traffic.logs.error";
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

        public async Task PublishAuditLogAsync(AuditLogMessage auditLog)
        {
            _logger.LogInformation("[AUDIT LOG] Publishing audit log: {Message}", auditLog.Message);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
            await sendEndpoint.Send(auditLog, context =>
            {
                context.SetRoutingKey(_auditLogRoutingKey);
            });
        }

        public async Task PublishErrorLogAsync(ErrorLogMessage errorLog)
        {
            _logger.LogError("[ERROR LOG] Publishing error log: {Message}", errorLog.Message);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
            await sendEndpoint.Send(errorLog, context =>
            {
                context.SetRoutingKey(_errorLogRoutingKey);
            });
        }

        private string BuildRoutingKey(string routingKeyBase, string intersectionId)
        {
            var baseKey = routingKeyBase.TrimEnd('*').TrimEnd('.');
            return $"{baseKey}.{intersectionId}";
        }
    }
}
