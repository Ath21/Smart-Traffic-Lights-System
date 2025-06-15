using System;
using LogStore.Business;
using LogStore.Models;
using MassTransit;
using UserMessages;

namespace LogStore.Consumers.User;

public class LogInfoConsumer : IConsumer<LogInfo>
{
    private readonly ILogService _logService;
    private readonly ILogger<LogInfoConsumer> _logger;

    public LogInfoConsumer(ILogService logService, ILogger<LogInfoConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
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

        _logger.LogInformation("LogInfoConsumer: {Message} at {Timestamp}", dto.Message, dto.Timestamp);

        await _logService.StoreLogAsync(dto);
    }
}
