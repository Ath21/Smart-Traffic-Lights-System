/*
 *  LogStore.Consumers.User.LogAuditConsumer
 *
 *  This class implements the IConsumer interface for handling LogAudit messages.
 *  It consumes messages related to user actions and logs audit information.
 *  The consumer uses the ILogService to store logs in the database.
 *  The message contains information about the action performed, details, and timestamp.
 *  The log message is formatted and stored in the database for later retrieval and analysis.
 */
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
