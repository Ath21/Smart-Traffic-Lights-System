using System;
using LogData.Collections;
using MongoDB.Driver;

namespace LogStore.Repository.Error;

public interface IErrorLogRepository
{
    Task<List<ErrorLog>> GetByServiceAsync(string serviceName);
    Task<List<ErrorLog>> GetByErrorTypeAsync(string errorType);
    Task CreateAsync(ErrorLog newLog);
    Task<List<ErrorLog>> FindAsync(FilterDefinition<ErrorLog> filter);
}