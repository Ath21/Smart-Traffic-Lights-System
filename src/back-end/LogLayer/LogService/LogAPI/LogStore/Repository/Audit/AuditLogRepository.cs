using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogStore.Repository.Audit;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly LogDbContext _context;

    public AuditLogRepository(LogDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(AuditLog newLog)
    {
        await _context.AuditLogs.InsertOneAsync(newLog);
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs.Find(_ => true).ToListAsync();
    }

    public async Task<List<AuditLog>> GetByServiceAsync(string serviceName)
    {
        return await _context.AuditLogs.Find(x => x.ServiceName == serviceName).ToListAsync();
    }

    public async Task<List<AuditLog>> FindAsync(FilterDefinition<AuditLog> filter)
{
    return await _context.AuditLogs.Find(filter).ToListAsync();
}
}
