using AutoMapper;
using LogData.Collections;
using LogStore.Models.Dtos;
using LogStore.Repository.Audit;
using LogStore.Repository.Error;
using MongoDB.Driver;

namespace LogStore.Business;

public class LogService : ILogService
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly IErrorLogRepository _errorRepo;
    private readonly IMapper _mapper;

    public LogService(IAuditLogRepository auditRepo, IErrorLogRepository errorRepo, IMapper mapper)
    {
        _auditRepo = auditRepo;
        _errorRepo = errorRepo;
        _mapper = mapper;
    }

    // ================================
    // STORE: Audit Log
    // ================================
    public async Task StoreAuditLogAsync(AuditLogDto logDto)
    {
        if (logDto.LogId == Guid.Empty)
            logDto.LogId = Guid.NewGuid();

        if (logDto.Timestamp == default)
            logDto.Timestamp = DateTime.UtcNow;

        var log = _mapper.Map<AuditLog>(logDto);
        await _auditRepo.CreateAsync(log);
    }

    // ================================
    // STORE: Error Log
    // (called from ExceptionMiddleware)
    // ================================
    public async Task StoreErrorLogAsync(ErrorLogDto logDto)
    {
        if (logDto.LogId == Guid.Empty)
            logDto.LogId = Guid.NewGuid();

        if (logDto.Timestamp == default)
            logDto.Timestamp = DateTime.UtcNow;

        var log = _mapper.Map<ErrorLog>(logDto);
        await _errorRepo.CreateAsync(log);
    }

    // ================================
    // GET: /audit/{serviceName}
    // ================================
    public async Task<List<AuditLogDto>> GetAuditLogsByServiceAsync(string serviceName)
    {
        var logs = await _auditRepo.GetByServiceAsync(serviceName);

        // Audit this operation
        await StoreAuditLogAsync(new AuditLogDto
        {
            LogId = Guid.NewGuid(),
            ServiceName = "LogService",
            Action = "GetAuditLogsByService",
            Message = $"Queried audit logs for service: {serviceName}",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "serviceQueried", serviceName } }
        });

        return _mapper.Map<List<AuditLogDto>>(logs);
    }

    // ================================
    // GET: /error/{serviceName}
    // ================================
    public async Task<List<ErrorLogDto>> GetErrorLogsByServiceAsync(string serviceName)
    {
        var logs = await _errorRepo.GetByServiceAsync(serviceName);

        // Audit this operation
        await StoreAuditLogAsync(new AuditLogDto
        {
            LogId = Guid.NewGuid(),
            ServiceName = "LogService",
            Action = "GetErrorLogsByService",
            Message = $"Queried error logs for service: {serviceName}",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "serviceQueried", serviceName } }
        });

        return _mapper.Map<List<ErrorLogDto>>(logs);
    }

    // ================================
    // GET: /search
    // ================================
    public async Task<List<object>> SearchLogsAsync(
        string? serviceName,
        string? errorType,
        string? action,
        DateTime? from,
        DateTime? to,
        Dictionary<string, string>? metadata)
    {
        var results = new List<object>();

        // ---- Audit Logs ----
        var auditFilter = Builders<AuditLog>.Filter.Empty;

        if (!string.IsNullOrEmpty(serviceName))
            auditFilter &= Builders<AuditLog>.Filter.Eq(x => x.ServiceName, serviceName);
        if (!string.IsNullOrEmpty(action))
            auditFilter &= Builders<AuditLog>.Filter.Eq(x => x.Action, action);
        if (from.HasValue)
            auditFilter &= Builders<AuditLog>.Filter.Gte(x => x.Timestamp, from.Value);
        if (to.HasValue)
            auditFilter &= Builders<AuditLog>.Filter.Lte(x => x.Timestamp, to.Value);

        var auditLogs = await _auditRepo.FindAsync(auditFilter);
        results.AddRange(_mapper.Map<List<AuditLogDto>>(auditLogs));

        // ---- Error Logs ----
        var errorFilter = Builders<ErrorLog>.Filter.Empty;

        if (!string.IsNullOrEmpty(serviceName))
            errorFilter &= Builders<ErrorLog>.Filter.Eq(x => x.ServiceName, serviceName);
        if (!string.IsNullOrEmpty(errorType))
            errorFilter &= Builders<ErrorLog>.Filter.Eq(x => x.ErrorType, errorType);
        if (from.HasValue)
            errorFilter &= Builders<ErrorLog>.Filter.Gte(x => x.Timestamp, from.Value);
        if (to.HasValue)
            errorFilter &= Builders<ErrorLog>.Filter.Lte(x => x.Timestamp, to.Value);

        var errorLogs = await _errorRepo.FindAsync(errorFilter);
        results.AddRange(_mapper.Map<List<ErrorLogDto>>(errorLogs));

        // Audit this operation
        await StoreAuditLogAsync(new AuditLogDto
        {
            LogId = Guid.NewGuid(),
            ServiceName = "LogService",
            Action = "SearchLogs",
            Message = "Performed a search across audit and error logs",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "serviceName", serviceName ?? "" },
                { "errorType", errorType ?? "" },
                { "action", action ?? "" },
                { "from", from?.ToString("O") ?? "" },
                { "to", to?.ToString("O") ?? "" }
            }
        });

        return results;
    }
}
