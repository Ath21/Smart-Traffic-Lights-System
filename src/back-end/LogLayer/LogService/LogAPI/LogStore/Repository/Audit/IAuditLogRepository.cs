using System;
using LogData.Collections;

namespace LogStore.Repository.Audit;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog newLog);
    Task<List<AuditLog>> GetAllAsync();
    Task<List<AuditLog>> GetByServiceAsync(string serviceName);
}
