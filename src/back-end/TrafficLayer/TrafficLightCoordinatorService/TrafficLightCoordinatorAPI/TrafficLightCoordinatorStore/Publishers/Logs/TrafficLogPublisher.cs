// Publishers/TrafficLogPublisher.cs
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrafficMessages.Logs;

namespace TrafficLightCoordinatorStore.Publishers.Logs;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _serviceName;
    private readonly string _logsExchange;
    private readonly string _auditKey;
    private readonly string _errorKey;

    public TrafficLogPublisher(IBus bus, IConfiguration config, ILogger<TrafficLogPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        var section = config.GetSection("RabbitMQ");
        _serviceName = "Traffic Light Coordination Service";
        _logsExchange = section["TrafficLogsExchange"] ?? "traffic.logs";
        _auditKey = section.GetSection("RoutingKeys")["Audit"] ?? "audit";
        _errorKey = section.GetSection("RoutingKeys")["Error"] ?? "error";
    }

    public async Task PublishAuditAsync(string message, CancellationToken ct)
    {
        _logger.LogInformation("[AUDIT] -> '{Exchange}' key '{Key}' | {Msg}", _logsExchange, _auditKey, message);

        await _bus.Publish(new AuditLogMessage(
            Service: _serviceName,
            Message: message,
            Timestamp: DateTime.UtcNow), ctx =>
        {
            ctx.SetRoutingKey(_auditKey);
        }, ct);
    }

    public async Task PublishErrorAsync(string message, Exception exception, CancellationToken ct)
    {
        _logger.LogWarning("[ERROR] -> '{Exchange}' key '{Key}' | {Msg}", _logsExchange, _errorKey, message);

        await _bus.Publish(new ErrorLogMessage(
            Service: _serviceName,
            Message: message,
            Exception: exception.ToString(),
            Timestamp: DateTime.UtcNow), ctx =>
        {
            ctx.SetRoutingKey(_errorKey);
        }, ct);
    }
}
