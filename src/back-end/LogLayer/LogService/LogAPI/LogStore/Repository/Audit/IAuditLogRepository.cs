using System;
using LogData.Collections;
using MongoDB.Driver;

namespace LogStore.Repository.Audit;


public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetByServiceAsync(string serviceName);
    Task CreateAsync(AuditLog newLog);
    Task<List<AuditLog>> FindAsync(FilterDefinition<AuditLog> filter);
}
