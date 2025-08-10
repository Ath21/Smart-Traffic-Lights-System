using System;
using System.Threading.Tasks;
using IntersectionControlStore.Publishers.LogPub;
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
        private readonly string _auditRoutingKey;
        private readonly string _errorRoutingKey;

        public TrafficLogPublisher(
            IBus bus,
            ILogger<TrafficLogPublisher> logger,
            IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _logStoreExchange = _configuration["RabbitMQ:Exchange:LogStoreExchange"] ?? "log.store.exchange";
            _auditRoutingKey = _configuration["RabbitMQ:RoutingKey:TrafficLogsAuditKey"] ?? "traffic.*.logs.audit";
            _errorRoutingKey = _configuration["RabbitMQ:RoutingKey:TrafficLogsErrorKey"] ?? "traffic.*.logs.error";
        }

        public async Task PublishAuditLogAsync(string serviceName, string message)
        {
            _logger.LogInformation("[TRAFFIC AUDIT] Publishing audit log to '{RoutingKey}'", _auditRoutingKey);

            var logMessage = new AuditLogMessage(serviceName, message, DateTime.UtcNow);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logStoreExchange}"));
            await sendEndpoint.Send(logMessage, context =>
            {
                context.SetRoutingKey(_auditRoutingKey);
            });

            _logger.LogInformation("[TRAFFIC AUDIT] Audit log published");
        }

        public async Task PublishErrorLogAsync(string serviceName, string message, Exception exception)
        {
            _logger.LogInformation("[TRAFFIC ERROR] Publishing error log to '{RoutingKey}'", _errorRoutingKey);

            var logMessage = new ErrorLogMessage(serviceName, message, exception.ToString(), DateTime.UtcNow);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logStoreExchange}"));
            await sendEndpoint.Send(logMessage, context =>
            {
                context.SetRoutingKey(_errorRoutingKey);
            });

            _logger.LogInformation("[TRAFFIC ERROR] Error log published");
        }
    }
}
