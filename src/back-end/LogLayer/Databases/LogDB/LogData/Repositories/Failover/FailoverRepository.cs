using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Failover;

public class FailoverRepository : IFailoverRepository
{
    private readonly LogDbContext _context;

    public FailoverRepository(LogDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(FailoverLog newLog)
    {
        await _context.FailoverLogs.InsertOneAsync(newLog);
    }

    public async Task<List<FailoverLog>> GetAllAsync()
    {
        return await _context.FailoverLogs.Find(_ => true).ToListAsync();
    }

    public async Task<List<FailoverLog>> GetByServiceAsync(string serviceName)
    {
        return await _context.FailoverLogs.Find(x => x.ServiceName == serviceName).ToListAsync();
    }

    public async Task<List<FailoverLog>> GetByContextAsync(string context)
    {
        return await _context.FailoverLogs.Find(x => x.Context == context).ToListAsync();
    }

    public async Task<List<FailoverLog>> FindAsync(FilterDefinition<FailoverLog> filter)
    {
        return await _context.FailoverLogs.Find(filter).ToListAsync();
    }
}
