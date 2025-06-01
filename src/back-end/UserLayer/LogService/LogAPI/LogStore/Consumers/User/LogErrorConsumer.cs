using System;
using LogStore.Business;
using LogStore.Models;
using MassTransit;
using UserMessages;

namespace LogStore.Consumers.User;

public class LogErrorConsumer : IConsumer<LogError>
{
    private readonly ILogService _logService;

    public LogErrorConsumer(ILogService logService)
    {
        _logService = logService;
    }

    public async Task Consume(ConsumeContext<LogError> context)
    {
        var dto = new LogDto
        {
            LogLevel = "ERROR",
            Message = $"[{context.Message.ErrorMessage}]\n{context.Message.StackTrace}",
            Timestamp = context.Message.Timestamp,
            Service = "User Service"
        };

        await _logService.StoreLogAsync(dto);
    }
}
