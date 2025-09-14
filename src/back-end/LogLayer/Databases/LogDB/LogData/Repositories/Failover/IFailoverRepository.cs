using System;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Failover;

public interface IFailoverRepository
{
    Task CreateAsync(FailoverLog newLog);
    Task<List<FailoverLog>> GetAllAsync();
    Task<List<FailoverLog>> GetByServiceAsync(string serviceName);
    Task<List<FailoverLog>> GetByContextAsync(string context);
    Task<List<FailoverLog>> FindAsync(FilterDefinition<FailoverLog> filter);
}
