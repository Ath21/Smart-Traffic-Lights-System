using System;
using LogStore.Business;

using LogStore.Models;
using MassTransit;
using UserMessages;

namespace LogStore.Consumers.User;

public class LogAuditConsumer : IConsumer<LogAudit>
{
    private readonly ILogService _logService;
    private readonly ILogger<LogAuditConsumer> _logger;

    public LogAuditConsumer(ILogService logService, ILogger<LogAuditConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LogAudit> context)
    {
        var dto = new LogDto
        {
            LogLevel = "AUDIT",
            Message = $"[{context.Message.Action}] {context.Message.Details}",
            Timestamp = context.Message.Timestamp,
            Service = "User Service"
        };

        _logger.LogInformation("LogAuditConsumer: {Message} at {Timestamp}", dto.Message, dto.Timestamp);

        await _logService.StoreLogAsync(dto);
    }
}
