using AutoMapper;
using LogData.Collections;
using LogStore.Models;
using LogStore.Models.Dtos;
using LogStore.Repository;
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

    // GET /audit/{serviceName}
    public async Task<List<AuditLogDto>> GetAuditLogsByServiceAsync(string serviceName)
    {
        var logs = await _auditRepo.GetByServiceAsync(serviceName);
        return _mapper.Map<List<AuditLogDto>>(logs);
    }

    // GET /error/{serviceName}
    public async Task<List<ErrorLogDto>> GetErrorLogsByServiceAsync(string serviceName)
    {
        var logs = await _errorRepo.GetByServiceAsync(serviceName);
        return _mapper.Map<List<ErrorLogDto>>(logs);
    }

    // GET /search → search across audit & error logs
    public async Task<List<object>> SearchLogsAsync(
        string? serviceName,
        string? errorType,
        string? action,
        DateTime? from,
        DateTime? to,
        Dictionary<string, string>? metadata)
    {
        var results = new List<object>();

        // Search Audit Logs
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

        // Search Error Logs
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

        return results;
    }
}