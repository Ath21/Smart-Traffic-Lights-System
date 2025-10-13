using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Audit;

public class AuditLogRepository : BaseLogRepository<AuditLogCollection>, IAuditLogRepository
{
    public AuditLogRepository(LogDbContext context)
        : base(context.AuditLogs)
    {
    }
}