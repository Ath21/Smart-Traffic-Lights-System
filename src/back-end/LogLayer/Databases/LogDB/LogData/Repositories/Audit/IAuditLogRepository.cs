using System;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Audit;


public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLogCollection>> GetAllAsync();
    Task<IEnumerable<AuditLogCollection>> GetByIntersectionAsync(int intersectionId);
    Task InsertAsync(AuditLogCollection log);
}
