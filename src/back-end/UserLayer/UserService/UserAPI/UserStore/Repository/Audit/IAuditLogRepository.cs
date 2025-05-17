using System;
using UserData.Entities;

namespace UserStore.Repository.Audit;

public interface IAuditLogRepository
{
    Task LogActionSync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId);
}
