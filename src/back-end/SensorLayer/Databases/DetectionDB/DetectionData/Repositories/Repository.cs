using System;
using MongoDB.Driver;

namespace DetectionData.Repositories;

public class Repository<T> : IRepository<T>
{
    private readonly IMongoCollection<T> _collection;

    public Repository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public async Task InsertAsync(T entity) =>
        await _collection.InsertOneAsync(entity);

    public async Task<List<T>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<T> GetLatestAsync(int intersectionId)
    {
        var filter = Builders<T>.Filter.Eq("intersection_id", intersectionId);
        return await _collection
            .Find(filter)
            .SortByDescending(x => (object)x.GetType().GetProperty("Timestamp").GetValue(x))
            .FirstOrDefaultAsync();
    }
}