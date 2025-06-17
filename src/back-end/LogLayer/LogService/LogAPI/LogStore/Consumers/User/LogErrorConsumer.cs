/*
 *  LogStore.Consumers.User.LogErrorConsumer
 *
 *  This class implements the IConsumer interface for handling LogError messages.
 *  It consumes messages related to error logging and stores the error details in the log service.
 *  The consumer uses the ILogService to store logs in the database.
 *  The message contains information about the error message, stack trace, and timestamp.
 *  The log message is formatted and stored in the database for later retrieval and analysis.
 */
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
