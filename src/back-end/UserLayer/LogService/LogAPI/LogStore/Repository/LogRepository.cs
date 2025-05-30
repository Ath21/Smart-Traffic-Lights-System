using LogData.Collections;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
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

    public async Task<List<Log?>> GetAsync(string Id)
    {
        return await _context.LogsCollection.Find(x => x.Id == Id).ToListAsync();
    }
}
