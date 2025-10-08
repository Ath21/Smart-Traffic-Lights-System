using System;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Error;

public interface IErrorLogRepository
{
    Task<IEnumerable<ErrorLogCollection>> GetAllAsync();
    Task<IEnumerable<ErrorLogCollection>> GetByServiceAsync(string service);
    Task InsertAsync(ErrorLogCollection log);
}