/*
 *  LogStore.Repository.ILogRepository
 *
 *  This interface defines the contract for a log repository that handles log operations.
 *  It includes methods for creating a new log, retrieving all logs, and retrieving logs by ID.
 *  The repository is responsible for interacting with the underlying data store to persist and retrieve logs.
 *  It is typically implemented by a class that interacts with a database or other storage mechanism.
 *  The methods are asynchronous to allow for non-blocking operations, which is important in web applications
 *  to ensure responsiveness and scalability.
 */
using LogData.Collections;

namespace LogStore.Repository;

public interface ILogRepository
{
    Task CreateAsync(Log newLog);
    Task<List<Log>> GetAllAsync();
    Task<List<Log?>> GetAsync(string Id);
}
