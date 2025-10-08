using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Audit;

public class AuditLogRepository : BaseRepository<AuditLogCollection>, IAuditLogRepository
{
    public AuditLogRepository(LogDbContext context)
        : base(context.AuditLogs) { }

    public async Task<IEnumerable<AuditLogCollection>> GetByIntersectionAsync(int intersectionId)
    {
        var filter = Builders<AuditLogCollection>.Filter.Eq(x => x.IntersectionId, intersectionId);
        var result = await _collection.FindAsync(filter);
        return await result.ToListAsync();
    }

    public new async Task<IEnumerable<AuditLogCollection>> GetAllAsync()
    {
        var result = await _collection.FindAsync(_ => true);
        return await result.ToListAsync();
    }

    public async Task InsertAsync(AuditLogCollection log)
    {
        await _collection.InsertOneAsync(log);
    }
}