using MassTransit;
using Messages.Log;
using LogData.Collections;
using LogData.Repositories.Audit;
using LogData.Repositories.Error;
using LogData.Repositories.Failover;
using MongoDB.Bson;
using LogData.Extensions;

namespace LogStore.Consumers;

public class LogConsumer : IConsumer<LogMessage>
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly IErrorLogRepository _errorRepo;
    private readonly IFailoverLogRepository _failoverRepo;
    private readonly ILogger<LogConsumer> _logger;

    public LogConsumer(
        IAuditLogRepository auditRepo,
        IErrorLogRepository errorRepo,
        IFailoverLogRepository failoverRepo,
        ILogger<LogConsumer> logger)
    {
        _auditRepo = auditRepo;
        _errorRepo = errorRepo;
        _failoverRepo = failoverRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LogMessage> context)
    {
        var msg = context.Message;

        if (string.IsNullOrWhiteSpace(msg.LogType))
        {
            _logger.LogWarning(
                "[CONSUMER] Skipping log message without LogType (CorrelationId={CorrelationId})",
                msg.CorrelationId);
            return;
        }

        // Convert metadata dictionary (if any)
        var metadataDoc = BsonExtensions.ToBsonDocument(msg.Metadata ?? new());

        switch (msg.LogType.ToLowerInvariant())
        {
            case "audit":
                var audit = new AuditLogCollection
                {
                    CorrelationId = msg.CorrelationId,
                    Timestamp = msg.Timestamp,

                    Layer = msg.SourceLayer,
                    Service = msg.SourceService,

                    Action = msg.Action,
                    Message = msg.Message,

                    Metadata = metadataDoc
                };

                await _auditRepo.InsertAsync(audit);
                _logger.LogInformation(
                    "[CONSUMER] Stored AUDIT log from {Service} ({Layer}) | CorrelationId={CorrelationId}",
                    audit.Service, audit.Layer, audit.CorrelationId);
                break;

            case "error":
                var error = new ErrorLogCollection
                {
                    CorrelationId = msg.CorrelationId,
                    Timestamp = msg.Timestamp,

                    Layer = msg.SourceLayer,
                    Service = msg.SourceService,

                    Action = msg.Action,
                    Message = msg.Message,

                    Metadata = metadataDoc
                };

                await _errorRepo.InsertAsync(error);
                _logger.LogInformation(
                    "[CONSUMER] Stored ERROR log from {Service} ({Layer}) | CorrelationId={CorrelationId}",
                    error.Service, error.Layer, error.CorrelationId);
                break;

            case "failover":
                var failover = new FailoverLogCollection
                {
                    CorrelationId = msg.CorrelationId,
                    Timestamp = msg.Timestamp,

                    Layer = msg.SourceLayer,
                    Service = msg.SourceService,

                    Action = msg.Action,
                    Message = msg.Message,

                    Metadata = metadataDoc
                };

                await _failoverRepo.InsertAsync(failover);
                _logger.LogInformation(
                    "[CONSUMER] Stored FAILOVER log from {Service} ({Layer}) | CorrelationId={CorrelationId}",
                    failover.Service, failover.Layer, failover.CorrelationId);
                break;
                
            default:
                _logger.LogWarning(
                    "[CONSUMER] Unknown log type '{Type}' from {Service} ({Layer}) | CorrelationId={CorrelationId}",
                    msg.LogType,
                    msg.SourceService,
                    msg.SourceLayer,
                    msg.CorrelationId);
                break;
        }
    }
}
