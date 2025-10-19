using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories;

public class BaseLogRepository<T> : IBaseLogRepository<T> where T : BaseLogCollection
{
    private readonly IMongoCollection<T> _collection;

    protected BaseLogRepository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    // ------------------------------------------------------------
    // Insert
    // ------------------------------------------------------------
    public async Task InsertAsync(T log)
    {
        await _collection.InsertOneAsync(log);
    }

    // ------------------------------------------------------------
    // Basic retrievals
    // ------------------------------------------------------------
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<T?> GetByCorrelationIdAsync(Guid correlationId)
    {
        var filter = Builders<T>.Filter.Eq(l => l.CorrelationId, correlationId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    // ------------------------------------------------------------
    // Search by source metadata
    // ------------------------------------------------------------
    public async Task<IEnumerable<T>> GetByLayerAsync(string sourceLayer)
    {
        var filter = Builders<T>.Filter.Eq(l => l.SourceLayer, sourceLayer);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetByServiceAsync(string sourceService)
    {
        var filter = Builders<T>.Filter.Eq(l => l.SourceService, sourceService);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetByLevelAsync(string sourceLevel)
    {
        var filter = Builders<T>.Filter.Eq(l => l.SourceLevel, sourceLevel);
        return await _collection.Find(filter).ToListAsync();
    }

    // ------------------------------------------------------------
    // Range queries
    // ------------------------------------------------------------
    public async Task<IEnumerable<T>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Gte(l => l.Timestamp, from),
            Builders<T>.Filter.Lte(l => l.Timestamp, to)
        );
        return await _collection.Find(filter).ToListAsync();
    }
}
