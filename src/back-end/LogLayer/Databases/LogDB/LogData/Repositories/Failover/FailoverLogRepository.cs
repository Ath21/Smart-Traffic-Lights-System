using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Failover;

public class FailoverLogRepository : BaseLogRepository<FailoverLogCollection>, IFailoverLogRepository
{
    public FailoverLogRepository(LogDbContext context)
        : base(context.FailoverLogs)
    {
    }
}
