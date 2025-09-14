using LogStore.Models;
using LogStore.Models.Dtos;

namespace LogStore.Business;

public interface ILogService
{
    Task<List<AuditLogDto>> GetAuditLogsByServiceAsync(string serviceName);
    Task<List<ErrorLogDto>> GetErrorLogsByServiceAsync(string serviceName);
    Task<List<object>> SearchLogsAsync(string? serviceName, string? errorType, string? action, DateTime? from, DateTime? to, Dictionary<string, string>? metadata);

    // New store methods
    Task StoreAuditLogAsync(AuditLogDto logDto);
    Task StoreErrorLogAsync(ErrorLogDto logDto);
    Task StoreFailoverLogAsync(FailoverLogDto log);
    Task<List<FailoverLogDto>> GetAllFailoverLogsAsync();
}
