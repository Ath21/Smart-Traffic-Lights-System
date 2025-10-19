using MassTransit;
using Messages.Log;
using LogData.Collections;
using MongoDB.Bson;
using LogData.Repositories.Audit;
using LogData.Repositories.Error;
using LogData.Repositories.Failover;

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
        var correlationId = Guid.TryParse(msg.CorrelationId, out var cid) ? cid : Guid.NewGuid();

        _logger.LogInformation(
            "[CONSUMER][LOG] Received log message from {Service} ({Layer}@{Level}) Type={Type}, CorrelationId={CorrelationId}",
            msg.Service, msg.Layer, msg.Level, msg.Type, correlationId);

        var bsonData = msg.Data is not null ? msg.Data.ToBsonDocument() : new BsonDocument();

        switch (msg.Type.ToLowerInvariant())
        {
            // =====================================================
            // AUDIT
            // =====================================================
            case "audit":
                var auditEntry = new AuditLogCollection
                {
                    AuditId = ObjectId.GenerateNewId().ToString(),
                    CorrelationId = correlationId,
                    Timestamp = msg.Timestamp.ToUniversalTime(),
                    SourceLayer = msg.Layer,
                    SourceLevel = msg.Level,
                    SourceService = msg.Service,
                    SourceDomain = msg.Domain,
                    Type = msg.Type,
                    Category = msg.Category,
                    Message = msg.Message,
                    Operation = msg.Operation,
                    Hostname = msg.Hostname,
                    ContainerIp = msg.ContainerIp,
                    Environment = msg.Environment,
                    Data = bsonData
                };

                _logger.LogInformation("[CONSUMER][LOG][AUDIT] Inserting AuditLog entry for {Service}", msg.Service);
                await _auditRepo.InsertAsync(auditEntry);
                _logger.LogInformation("[CONSUMER][LOG][AUDIT] Inserted AuditLog with Id={AuditId}", auditEntry.AuditId);
                break;

            // =====================================================
            // ERROR
            // =====================================================
            case "error":
                var errorEntry = new ErrorLogCollection
                {
                    ErrorId = ObjectId.GenerateNewId().ToString(),
                    CorrelationId = correlationId,
                    Timestamp = msg.Timestamp.ToUniversalTime(),
                    SourceLayer = msg.Layer,
                    SourceLevel = msg.Level,
                    SourceService = msg.Service,
                    SourceDomain = msg.Domain,
                    Type = msg.Type,
                    Category = msg.Category,
                    Message = msg.Message,
                    Operation = msg.Operation,
                    Hostname = msg.Hostname,
                    ContainerIp = msg.ContainerIp,
                    Environment = msg.Environment,
                    Data = bsonData
                };

                _logger.LogWarning("[CONSUMER][LOG][ERROR] Inserting ErrorLog entry for {Service}", msg.Service);
                await _errorRepo.InsertAsync(errorEntry);
                _logger.LogWarning("[CONSUMER][LOG][ERROR] Inserted ErrorLog with Id={ErrorId}", errorEntry.ErrorId);
                break;

            // =====================================================
            // FAILOVER
            // =====================================================
            case "failover":
                var failoverEntry = new FailoverLogCollection
                {
                    FailoverId = ObjectId.GenerateNewId().ToString(),
                    CorrelationId = correlationId,
                    Timestamp = msg.Timestamp.ToUniversalTime(),
                    SourceLayer = msg.Layer,
                    SourceLevel = msg.Level,
                    SourceService = msg.Service,
                    SourceDomain = msg.Domain,
                    Type = msg.Type,
                    Category = msg.Category,
                    Message = msg.Message,
                    Operation = msg.Operation,
                    Hostname = msg.Hostname,
                    ContainerIp = msg.ContainerIp,
                    Environment = msg.Environment,
                    Data = bsonData
                };

                _logger.LogInformation("[CONSUMER][LOG][FAILOVER] Inserting FailoverLog entry for {Service}", msg.Service);
                await _failoverRepo.InsertAsync(failoverEntry);
                _logger.LogInformation("[CONSUMER][LOG][FAILOVER] Inserted FailoverLog with Id={FailoverId}", failoverEntry.FailoverId);
                break;

            // =====================================================
            // DEFAULT → AUDIT
            // =====================================================
            default:
                var defaultEntry = new AuditLogCollection
                {
                    AuditId = ObjectId.GenerateNewId().ToString(),
                    CorrelationId = correlationId,
                    Timestamp = msg.Timestamp.ToUniversalTime(),
                    SourceLayer = msg.Layer,
                    SourceLevel = msg.Level,
                    SourceService = msg.Service,
                    SourceDomain = msg.Domain,
                    Type = msg.Type,
                    Category = msg.Category,
                    Message = msg.Message,
                    Operation = msg.Operation,
                    Hostname = msg.Hostname,
                    ContainerIp = msg.ContainerIp,
                    Environment = msg.Environment,
                    Data = bsonData
                };

                _logger.LogInformation("[CONSUMER][LOG][DEFAULT] No matching type; saving as AuditLog for {Service}", msg.Service);
                await _auditRepo.InsertAsync(defaultEntry);
                _logger.LogInformation("[CONSUMER][LOG][DEFAULT] Inserted fallback AuditLog with Id={AuditId}", defaultEntry.AuditId);
                break;
        }

        _logger.LogInformation("[CONSUMER][LOG] Stored {Type} log from {Service} ({Layer}@{Level}) successfully",
            msg.Type, msg.Service, msg.Layer, msg.Level);
    }
}
