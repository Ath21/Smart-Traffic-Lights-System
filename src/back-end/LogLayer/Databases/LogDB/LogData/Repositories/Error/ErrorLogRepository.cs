using LogData;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Error;

public class ErrorLogRepository : BaseRepository<ErrorLogCollection>, IErrorLogRepository
{
    public ErrorLogRepository(LogDbContext context)
        : base(context.ErrorLogs) { }

    public async Task<IEnumerable<ErrorLogCollection>> GetByServiceAsync(string service)
    {
        var filter = Builders<ErrorLogCollection>.Filter.Eq(x => x.Service, service);
        var result = await _collection.FindAsync(filter);
        return await result.ToListAsync();
    }

    public new async Task<IEnumerable<ErrorLogCollection>> GetAllAsync()
    {
        var result = await _collection.FindAsync(_ => true);
        return await result.ToListAsync();
    }

    public async Task InsertAsync(ErrorLogCollection log)
    {
        await _collection.InsertOneAsync(log);
    }
}