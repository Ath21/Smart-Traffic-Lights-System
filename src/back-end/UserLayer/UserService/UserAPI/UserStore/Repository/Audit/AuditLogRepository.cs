/*
 * UserStore.Repository.Audit.AuditLogRepository
 *
 * This class implements the IAuditLogRepository interface and provides methods for logging user actions
 * and retrieving audit logs.
 * It uses Entity Framework Core to interact with the database.
 * The methods include:
 * - LogActionSync: Logs a user action synchronously.
 * - GetAuditLogsByUserIdAsync: Retrieves audit logs for a specific user by their unique identifier (UserId).
 * The class uses the UserDbContext to interact with the database and perform the necessary operations.
 * The AuditLogRepository class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
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
