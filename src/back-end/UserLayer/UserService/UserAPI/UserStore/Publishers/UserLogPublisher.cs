using System;
using MassTransit;
using UserStore.Messages;

namespace UserStore.Publishers;

public class UserLogPublisher : IUserLogPublisher
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IConfiguration _configuration;

    private readonly string _exchangeName;
    private readonly string _infoKey;
    private readonly string _auditKey;
    private readonly string _errorKey;

    public UserLogPublisher(ISendEndpointProvider sendEndpointProvider, IConfiguration configuration)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _configuration = configuration;

        var section = configuration.GetSection("RabbitMQ");
        _exchangeName = section["UserLogsExchange"];
        _infoKey = section["RoutingKeys:Info"];
        _auditKey = section["RoutingKeys:Audit"];
        _errorKey = section["RoutingKeys:Error"];
    }

    public async Task PublishAuditAsync(Guid userId, string action, string details)
    {
        var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"exchange:{_exchangeName}"));
        await endpoint.Send(new LogAudit(
            userId,
            action,
            details,
            DateTime.UtcNow), context =>
            {
                context.SetRoutingKey(_auditKey);
            });
    }

    public async Task PublishErrorAsync(string message, Exception exception)
    {
        var endpoing = await _sendEndpointProvider.GetSendEndpoint(new Uri($"exchange:{_exchangeName}"));
        await endpoing.Send(new LogError(
            message,
            exception.StackTrace ?? "No stack trace",
            DateTime.UtcNow), context =>
            {
                context.SetRoutingKey(_errorKey);
            });
    }

    public async Task PublishInfoAsync(string message)
    {
        var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"exchange:{_exchangeName}"));
        await endpoint.Send(new LogInfo(
            message,
            DateTime.UtcNow), context =>
            {
                context.SetRoutingKey(_infoKey);
            });
    }
}
