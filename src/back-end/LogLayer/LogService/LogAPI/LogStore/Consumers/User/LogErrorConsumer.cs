using System;
using LogStore.Business;
using LogStore.Models;
using MassTransit;
using UserMessages;

namespace LogStore.Consumers.User;

public class LogErrorConsumer : IConsumer<LogError>
{
    private readonly ILogService _logService;
    private readonly ILogger<LogErrorConsumer> _logger;

    public LogErrorConsumer(ILogService logService, ILogger<LogErrorConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
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

        _logger.LogError("LogErrorConsumer: {Message} at {Timestamp}", dto.Message, dto.Timestamp);

        await _logService.StoreLogAsync(dto);
    }
}
