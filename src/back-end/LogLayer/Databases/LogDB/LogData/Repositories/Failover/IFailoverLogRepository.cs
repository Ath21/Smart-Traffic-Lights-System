using System;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Failover;

public interface IFailoverLogRepository
{
    Task<IEnumerable<FailoverLogCollection>> GetAllAsync();
    Task<IEnumerable<FailoverLogCollection>> GetByReasonAsync(string reason);
    Task InsertAsync(FailoverLogCollection log);
}
