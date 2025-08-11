using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages.Logs;

namespace IntersectionControlStore.Publishers.LogPub
{
    public class TrafficLogPublisher : ITrafficLogPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<TrafficLogPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _logStoreExchange;
        private readonly string _auditRoutingKeyBase;
        private readonly string _errorRoutingKeyBase;

        public TrafficLogPublisher(
            IBus bus,
            ILogger<TrafficLogPublisher> logger,
            IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _logStoreExchange = _configuration["RabbitMQ:Exchange:LogStoreExchange"] ?? "log.store.exchange";

            // Expect routing keys with trailing wildcard to append intersection or service id dynamically
            _auditRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:TrafficLogsAuditKey"] ?? "traffic.*.logs.audit";
            _errorRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:TrafficLogsErrorKey"] ?? "traffic.*.logs.error";
        }

        public async Task PublishAuditLogAsync(string serviceName, string message, string intersectionId = null)
        {
            var routingKey = BuildRoutingKey(_auditRoutingKeyBase, intersectionId ?? "general");

            _logger.LogInformation("[TRAFFIC AUDIT] Publishing audit log to '{RoutingKey}'", routingKey);

            var logMessage = new AuditLogMessage(serviceName, message, DateTime.UtcNow);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logStoreExchange}"));
            await sendEndpoint.Send(logMessage, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[TRAFFIC AUDIT] Audit log published");
        }

        public async Task PublishErrorLogAsync(string serviceName, string message, Exception exception, string intersectionId = null)
        {
            var routingKey = BuildRoutingKey(_errorRoutingKeyBase, intersectionId ?? "general");

            _logger.LogInformation("[TRAFFIC ERROR] Publishing error log to '{RoutingKey}'", routingKey);

            var logMessage = new ErrorLogMessage(serviceName, message, exception.ToString(), DateTime.UtcNow);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logStoreExchange}"));
            await sendEndpoint.Send(logMessage, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[TRAFFIC ERROR] Error log published");
        }

        public Task PublishErrorLogAsync(string serviceName, string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        private string BuildRoutingKey(string routingKeyBase, string id)
        {
            // Replace wildcard * or trim it and append id
            if (routingKeyBase.Contains("*"))
            {
                return routingKeyBase.Replace("*", id);
            }

            var baseKey = routingKeyBase.TrimEnd('*').TrimEnd('.');
            return $"{baseKey}.{id}";
        }
    }
}
