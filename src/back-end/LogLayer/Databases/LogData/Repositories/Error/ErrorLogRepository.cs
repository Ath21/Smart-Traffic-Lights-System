using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Error;

public class ErrorLogRepository : IErrorLogRepository
{
    private readonly LogDbContext _context;

    public ErrorLogRepository(LogDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(ErrorLog newLog)
    {
        await _context.ErrorLogs.InsertOneAsync(newLog);
    }

    public async Task<List<ErrorLog>> GetAllAsync()
    {
        return await _context.ErrorLogs.Find(_ => true).ToListAsync();
    }

    public async Task<List<ErrorLog>> GetByServiceAsync(string serviceName)
    {
        return await _context.ErrorLogs.Find(x => x.ServiceName == serviceName).ToListAsync();
    }

    public async Task<List<ErrorLog>> GetByErrorTypeAsync(string errorType)
    {
        return await _context.ErrorLogs.Find(x => x.ErrorType == errorType).ToListAsync();
    }

    public async Task<List<ErrorLog>> FindAsync(FilterDefinition<ErrorLog> filter)
{
    return await _context.ErrorLogs.Find(filter).ToListAsync();
}
}