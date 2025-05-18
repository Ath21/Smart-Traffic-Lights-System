/*
 * UserStore.Repository.Audit.IAuditLogRepository
 *
 * This interface defines the contract for the AuditLog repository in the UserStore application.
 * It contains methods for logging user actions and retrieving audit logs.
 * The methods include:
 * - LogActionSync: Logs a user action synchronously.
 * - GetAuditLogsByUserIdAsync: Retrieves audit logs for a specific user by their unique identifier (UserId).
 * The IAuditLogRepository interface is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using UserData.Entities;

namespace UserStore.Repository.Audit;

public interface IAuditLogRepository
{
    Task LogActionSync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId);
}
