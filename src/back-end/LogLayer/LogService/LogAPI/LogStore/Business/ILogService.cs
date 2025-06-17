/*
 *  LogStore.Business.ILogService
 *
 *  This interface defines the contract for log management services.
 *  It includes methods for storing logs, retrieving all logs, and retrieving logs by service.
 *  Implementations of this interface will handle the business logic related to log management.
 *  The methods are asynchronous to support non-blocking operations.
 *  The LogDto model is used to transfer log data between the service and the data layer
 */
using LogStore.Models;

namespace LogStore.Business;

public interface ILogService
{
    Task StoreLogAsync(LogDto logDto);
    Task<List<LogDto>> GetAllLogsAsync();
    Task<List<LogDto>> GetLogsByServiceAsync(string service);
}
