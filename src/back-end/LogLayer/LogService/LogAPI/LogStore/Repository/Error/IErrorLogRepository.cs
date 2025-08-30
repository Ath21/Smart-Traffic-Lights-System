using System;
using LogData.Collections;

namespace LogStore.Repository.Error;

public interface IErrorLogRepository
{
    Task CreateAsync(ErrorLog newLog);
    Task<List<ErrorLog>> GetAllAsync();
    Task<List<ErrorLog>> GetByServiceAsync(string serviceName);
    Task<List<ErrorLog>> GetByErrorTypeAsync(string errorType);
}