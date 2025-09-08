using UserData.Entities;

namespace UserData.Repositories.Audit;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
}
