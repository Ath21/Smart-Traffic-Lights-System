using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Failover;

public class FailoverLogRepository : BaseRepository<FailoverLogCollection>, IFailoverLogRepository
{
    public FailoverLogRepository(LogDbContext context)
        : base(context.FailoverLogs) { }

    public async Task<IEnumerable<FailoverLogCollection>> GetByReasonAsync(string reason)
    {
        var filter = Builders<FailoverLogCollection>.Filter.Eq(x => x.Reason, reason);
        var result = await _collection.FindAsync(filter);
        return await result.ToListAsync();
    }

    public new async Task<IEnumerable<FailoverLogCollection>> GetAllAsync()
    {
        var result = await _collection.FindAsync(_ => true);
        return await result.ToListAsync();
    }

    public async Task InsertAsync(FailoverLogCollection log)
    {
        await _collection.InsertOneAsync(log);
    }
}
