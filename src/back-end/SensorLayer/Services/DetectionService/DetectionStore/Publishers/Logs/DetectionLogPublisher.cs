using System.Net;
using System.Net.Sockets;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Log;

namespace DetectionStore.Publishers.Logs;

public class DetectionLogPublisher : IDetectionLogPublisher
{
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<DetectionLogPublisher> _logger;
    private readonly string _routingPattern;
    private readonly string _exchangeName;

    private const string Tag = "[PUBLISHER][LOG]";

    // Cached environment context
    private readonly string _layer;
    private readonly string _level;
    private readonly string _service;
    private readonly string _environment;
    private readonly string _hostname;
    private readonly string _containerIp;

    public DetectionLogPublisher(
        IPublishEndpoint publisher,
        IConfiguration configuration,
        ILogger<DetectionLogPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;

        _exchangeName = configuration["RabbitMQ:Exchanges:Log"] ?? "log.exchange";
        _routingPattern = configuration["RabbitMQ:RoutingKeys:Log:Detection"] ?? "log.sensor.detection-api.{type}";

        _layer = Environment.GetEnvironmentVariable("SERVICE_LAYER") ?? "Sensor";
        _level = Environment.GetEnvironmentVariable("SERVICE_LEVEL") ?? "Fog";
        _service = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Detection Service";
        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        _hostname = Environment.MachineName;
        _containerIp = Dns.GetHostAddresses(Dns.GetHostName())
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            ?.ToString() ?? "unknown";
    }

    public async Task PublishAuditAsync(
        string domain,
        string messageText,
        string? category = "system",
        Dictionary<string, object>? data = null,
        string? operation = null)
    {
        var routingKey = _routingPattern.Replace("{type}", "audit");

        var msg = new LogMessage
        {
            Layer = _layer,
            Level = _level,
            Service = _service,
            Domain = domain,
            Type = "audit",
            Category = category ?? "system",
            Message = messageText,
            Operation = operation,
            Hostname = _hostname,
            ContainerIp = _containerIp,
            Environment = _environment,
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        try
        {
            await _publisher.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
            _logger.LogInformation("{Tag} Published AUDIT log | Domain={Domain} | Message={Message}", Tag, domain, messageText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish AUDIT log | Domain={Domain} | Error={Error}", Tag, domain, ex.Message);
        }
    }

    public async Task PublishErrorAsync(
        string domain,
        string messageText,
        Dictionary<string, object>? data = null,
        string? operation = null)
    {
        var routingKey = _routingPattern.Replace("{type}", "error");

        var msg = new LogMessage
        {
            Layer = _layer,
            Level = _level,
            Service = _service,
            Domain = domain,
            Type = "error",
            Category = "Error",
            Message = messageText,
            Operation = operation,
            Hostname = _hostname,
            ContainerIp = _containerIp,
            Environment = _environment,
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        try
        {
            await _publisher.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
            _logger.LogWarning("{Tag} Published ERROR log | Domain={Domain} | Message={Message}", Tag, domain, messageText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish ERROR log | Domain={Domain} | Error={Error}", Tag, domain, ex.Message);
        }
    }

    public async Task PublishFailoverAsync(
        string domain,
        string messageText,
        Dictionary<string, object>? data = null,
        string? operation = null)
    {
        var routingKey = _routingPattern.Replace("{type}", "failover");

        var msg = new LogMessage
        {
            Layer = _layer,
            Level = _level,
            Service = _service,
            Domain = domain,
            Type = "failover",
            Category = "Failover",
            Message = messageText,
            Operation = operation,
            Hostname = _hostname,
            ContainerIp = _containerIp,
            Environment = _environment,
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        try
        {
            await _publisher.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
            _logger.LogWarning("{Tag} Published FAILOVER log | Domain={Domain} | Message={Message}", Tag, domain, messageText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish FAILOVER log | Domain={Domain} | Error={Error}", Tag, domain, ex.Message);
        }
    }
}
