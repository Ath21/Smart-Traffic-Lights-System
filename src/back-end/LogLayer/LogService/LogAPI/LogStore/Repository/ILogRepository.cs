using System;
using LogData.Collections;

namespace LogStore.Repository;

public interface ILogRepository
{
    Task CreateAsync(Log newLog);
    Task<List<Log>> GetAllAsync();
    Task<List<Log?>> GetAsync(string Id);
}
