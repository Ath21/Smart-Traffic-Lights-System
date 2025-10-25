using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Log;

namespace TrafficLightCoordinatorStore.Publishers.Logs
{
    public class CoordinatorLogPublisher : ICoordinatorLogPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<CoordinatorLogPublisher> _logger;
        private readonly string _routingPattern;

        public CoordinatorLogPublisher(
            IBus bus,
            IConfiguration config,
            ILogger<CoordinatorLogPublisher> logger)
        {
            _bus = bus;
            _logger = logger;

            _routingPattern = config["RabbitMQ:RoutingKeys:Log:Coordinator"]
                              ?? "log.traffic.light-coordinator.{type}";
        }

        public async Task PublishAuditAsync(
            string operation,
            string message,
            string domain = "[COORDINATOR]",
            string category = "system",
            Dictionary<string, object>? data = null)
        {
            await PublishAsync("audit", operation, message, domain, category, data);
            _logger.LogInformation("[PUBLISHER][LOG][AUDIT] {Operation}: {Message}", operation, message);
        }

        public async Task PublishErrorAsync(
            string operation,
            string message,
            Exception? ex = null,
            string domain = "[COORDINATOR]",
            string category = "system",
            Dictionary<string, object>? data = null)
        {
            data ??= new();
            if (ex != null)
            {
                data["exception_type"] = ex.GetType().Name;
                data["exception_message"] = ex.Message;
            }

            await PublishAsync("error", operation, message, domain, category, data);
            _logger.LogError("[PUBLISHER][LOG][ERROR] {Operation}: {Message}", operation, message);
        }

        public async Task PublishFailoverAsync(
            string operation,
            string message,
            string domain = "[COORDINATOR]",
            string category = "system",
            Dictionary<string, object>? data = null)
        {
            await PublishAsync("failover", operation, message, domain, category, data);
            _logger.LogWarning("[PUBLISHER][LOG][FAILOVER] {Operation}: {Message}", operation, message);
        }

        private async Task PublishAsync(
            string type,
            string operation,
            string message,
            string domain,
            string category,
            Dictionary<string, object>? data)
        {
            var msg = new LogMessage
            {
                Layer = "Traffic",
                Level = "Cloud",
                Service = "TrafficLightCoordinator",
                Domain = domain,
                Type = type,
                Category = category,
                Message = message,
                Operation = operation,
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Data = data
            };

            var routingKey = _routingPattern.Replace("{type}", type);
            await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
        }
    }
}
