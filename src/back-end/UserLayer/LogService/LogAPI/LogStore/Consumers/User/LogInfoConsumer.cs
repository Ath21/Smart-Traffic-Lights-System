using System;
using LogStore.Business;
using LogStore.Messages.User;
using LogStore.Models;
using MassTransit;

namespace LogStore.Consumers.User;

public class LogInfoConsumer : IConsumer<LogInfo>
{
    private readonly ILogService _logService;

    public LogInfoConsumer(ILogService logService)
    {
        _logService = logService;
    }

    public async Task Consume(ConsumeContext<LogInfo> context)
    {
        var dto = new LogDto
        {
            LogLevel = "INFO",
            Message = context.Message.Message,
            Timestamp = context.Message.Timestamp,
            Service = "User Service"
        };

        Console.WriteLine($"LogInfoConsumer: {dto.Message} at {dto.Timestamp}");

        await _logService.StoreLogAsync(dto);
    }
}
