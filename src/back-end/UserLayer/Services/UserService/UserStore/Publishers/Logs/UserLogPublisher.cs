using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Log;

namespace UserStore.Publishers.Logs;

public class UserLogPublisher : IUserLogPublisher
{
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<UserLogPublisher> _logger;
    private readonly string _routingPattern;
    private readonly string _exchangeName;

    private const string Tag = "[PUBLISHER][LOG]";

    public UserLogPublisher(IPublishEndpoint publisher, IConfiguration configuration, ILogger<UserLogPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;

        _exchangeName = configuration["RabbitMQ:Exchanges:Log"] ?? "LOG.EXCHANGE";
        _routingPattern = configuration["RabbitMQ:RoutingKeys:Log:User"] ?? "log.user.user-api.{type}";
    }

    public async Task PublishAuditAsync(string source, string messageText, string? category = null, Dictionary<string, object>? data = null, string level = "info")
    {
        var routingKey = _routingPattern.Replace("{type}", "audit");

        var msg = new LogMessage
        {
            Source = source,
            Type = "audit",
            Message = messageText,
            Category = category,
            Level = level,
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        try
        {
            await _publisher.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
            _logger.LogInformation("{Tag} Published audit log: {Message}", Tag, messageText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish audit log: {Message}", Tag, ex.Message);
        }
    }

    public async Task PublishErrorAsync(string source, string messageText, Dictionary<string, object>? data = null)
    {
        var routingKey = _routingPattern.Replace("{type}", "error");

        var msg = new LogMessage
        {
            Source = source,
            Type = "error",
            Message = messageText,
            Category = "Error",
            Level = "error",
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        try
        {
            await _publisher.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
            _logger.LogWarning("{Tag} Published error log: {Message}", Tag, messageText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish error log: {Message}", Tag, ex.Message);
        }
    }
}
