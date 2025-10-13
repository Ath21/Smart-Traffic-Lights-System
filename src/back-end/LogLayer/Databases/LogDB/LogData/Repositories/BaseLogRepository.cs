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

    public async Task InsertAsync(T log)
    {
        await _collection.InsertOneAsync(log);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<T?> GetByCorrelationIdAsync(Guid correlationId)
    {
        var filter = Builders<T>.Filter.Eq(l => l.CorrelationId, correlationId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetByLayerAsync(string layer)
    {
        var filter = Builders<T>.Filter.Eq(l => l.Layer, layer);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetByServiceAsync(string service)
    {
        var filter = Builders<T>.Filter.Eq(l => l.Service, service);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Gte(l => l.Timestamp, from),
            Builders<T>.Filter.Lte(l => l.Timestamp, to)
        );
        return await _collection.Find(filter).ToListAsync();
    }
}
