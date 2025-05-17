using System;
using UserData;
using UserData.Entities;

namespace UserStore.Repository.Audit;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly UserDbContext _context;

    public AuditLogRepository(UserDbContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId)
    {
        var auditLogs = _context.AuditLogs.Where(log => log.UserId == userId).ToList();
        return Task.FromResult<IEnumerable<AuditLog>>(auditLogs);
    }

    public Task LogActionSync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        return _context.SaveChangesAsync();
    }
}
