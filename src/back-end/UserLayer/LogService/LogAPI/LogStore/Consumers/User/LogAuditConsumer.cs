using System;
using LogStore.Business;
using LogStore.Messages.User;
using LogStore.Models;
using MassTransit;

namespace LogStore.Consumers.User;

public class LogAuditConsumer : IConsumer<LogAudit>
{
    private readonly ILogService _logService;

    public LogAuditConsumer(ILogService logService)
    {
        _logService = logService;
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

        await _logService.StoreLogAsync(dto);
    }
}
