/*
 *  LogStore.Repository.LogRepository
 *
 * This class implements the ILogRepository interface for handling log operations.
 * It provides methods to create a new log, retrieve all logs, and retrieve logs by service name.
 * The repository interacts with the MongoDB database to perform these operations.
 */
using LogData.Collections;
using MongoDB.Driver;
using LogData;

namespace LogStore.Repository;

public class LogRepository : ILogRepository
{
    private readonly LogDbContext _context;

    public LogRepository(LogDbContext context)
    {
        _context = context; 
    }

    public async Task CreateAsync(Log newLog)
    {
        await _context.LogsCollection.InsertOneAsync(newLog);
    }

    public async Task<List<Log>> GetAllAsync()
    {
        return await _context.LogsCollection.Find(_ => true).ToListAsync();
    }

    public async Task<List<Log?>> GetAsync(string Service)
    {
        return await _context.LogsCollection.Find(x => x.Service == Service).ToListAsync();
    }
}
