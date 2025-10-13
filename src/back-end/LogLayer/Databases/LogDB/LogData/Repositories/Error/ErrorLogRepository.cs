using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Error;

public class ErrorLogRepository : BaseLogRepository<ErrorLogCollection>, IErrorLogRepository
{
    public ErrorLogRepository(LogDbContext context)
        : base(context.ErrorLogs)
    {
    }
}