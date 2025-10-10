using AutoMapper;
using MassTransit;
using Messages.Log;
using LogData.Collections;
using LogData.Repositories.Audit;
using LogData.Repositories.Error;
using LogData.Repositories.Failover;

namespace LogStore.Consumers;

public class LogConsumer : IConsumer<LogMessage>
{
    private readonly IMapper _mapper;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IErrorLogRepository _errorRepo;
    private readonly IFailoverLogRepository _failoverRepo;
    private readonly ILogger<LogConsumer> _logger;

    public LogConsumer(
        IMapper mapper,
        IAuditLogRepository auditRepo,
        IErrorLogRepository errorRepo,
        IFailoverLogRepository failoverRepo,
        ILogger<LogConsumer> logger)
    {
        _mapper = mapper;
        _auditRepo = auditRepo;
        _errorRepo = errorRepo;
        _failoverRepo = failoverRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LogMessage> context)
    {
        var msg = context.Message;
        if (string.IsNullOrEmpty(msg.LogType))
        {
            _logger.LogWarning("[CONSUMER] Skipping log message without LogType (CorrelationId={CorrelationId})", msg.CorrelationId);
            return;
        }

        switch (msg.LogType.ToLowerInvariant())
        {
            case "audit":
                var audit = _mapper.Map<AuditLogCollection>(msg);
                await _auditRepo.InsertAsync(audit);
                _logger.LogInformation("[CONSUMER] Stored AUDIT log from {Service} ({Layer})", audit.Service, audit.Layer);
                break;

            case "error":
                var error = _mapper.Map<ErrorLogCollection>(msg);
                await _errorRepo.InsertAsync(error);
                _logger.LogInformation("[CONSUMER] Stored ERROR log from {Service} ({Layer})", error.Service, error.Layer);
                break;

            case "failover":
                var failover = _mapper.Map<FailoverLogCollection>(msg);
                await _failoverRepo.InsertAsync(failover);
                _logger.LogInformation("[CONSUMER] Stored FAILOVER log from {Service} ({Layer})", failover.Service, failover.Layer);
                break;

            default:
                _logger.LogWarning("[CONSUMER] Unknown log type '{Type}' from {Service}", msg.LogType, msg.Layer);
                break;
        }
    }
}
