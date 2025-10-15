using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Log;

namespace UserStore.Publishers.Logs;

public class UserLogPublisher : IUserLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<UserLogPublisher> _logger;
    private readonly string _routingPattern;

    public UserLogPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<UserLogPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:User"]
                          ?? "log.user.user-service.{type}";
    }

    public async Task PublishAuditAsync(string action, string message, Dictionary<string, string>? metadata = null)
        => await PublishAsync("audit", "Audit", action, message, metadata);

    public async Task PublishErrorAsync(string action, string message, Exception? ex = null, Dictionary<string, string>? metadata = null)
    {
        if (ex != null)
        {
            metadata ??= new();
            metadata["exception_type"] = ex.GetType().Name;
            metadata["exception_message"] = ex.Message;
        }

        await PublishAsync("error", "Error", action, message, metadata);
    }

    public async Task PublishFailoverAsync(string action, string message, Dictionary<string, string>? metadata = null)
        => await PublishAsync("failover", "Failover", action, message, metadata);

    private async Task PublishAsync(
        string routingSuffix,
        string logType,
        string action,
        string message,
        Dictionary<string, string>? metadata)
    {
        var msg = new LogMessage
        {
            CorrelationId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "User Layer",
            DestinationLayer = new() { "Log Layer" },

            SourceService = "User Service",
            DestinationServices = new() { "Log Service" },

            LogType = logType,
            Action = action,
            Message = message,
            Metadata = metadata
        };

        var routingKey = _routingPattern.Replace("{type}", routingSuffix);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISHER][LOG][{Type}] {Action} - {Message}", logType, action, message);
    }
}
