using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Audit;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly UserDbContext _context;

    public AuditLogRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AuditLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}
