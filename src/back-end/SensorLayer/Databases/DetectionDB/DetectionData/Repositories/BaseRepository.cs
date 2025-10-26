using System;
using System.Linq.Expressions;
using MongoDB.Driver;

namespace DetectionData.Repositories;

public abstract class BaseRepository<T>
{
    protected readonly IMongoCollection<T> _collection;
    private const string domain = "[REPOSITORY][DETECTION]";
    private readonly ILogger<BaseRepository<T>> _logger;

    protected BaseRepository(IMongoCollection<T> collection, ILogger<BaseRepository<T>> logger)
    {
        _collection = collection;
        _logger = logger;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        _logger.LogInformation("{Domain} Retrieving all documents\n", domain);
        var result = await _collection.FindAsync(_ => true);
        return await result.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id, Expression<Func<T, string?>> idSelector)
    {
        _logger.LogInformation("{Domain} Retrieving document with ID {Id}\n", domain, id);
        var filter = Builders<T>.Filter.Eq(idSelector, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        _logger.LogInformation("{Domain} Finding documents matching predicate\n", domain);
        var result = await _collection.FindAsync(predicate);
        return await result.ToListAsync();
    }

    public virtual async Task InsertAsync(T entity)
    {
        _logger.LogInformation("{Domain} Inserting document\n", domain);
        await _collection.InsertOneAsync(entity);
    }

    public virtual async Task InsertManyAsync(IEnumerable<T> entities)
    {
        _logger.LogInformation("{Domain} Inserting multiple documents\n", domain);
        await _collection.InsertManyAsync(entities);
    }

    public virtual async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        _logger.LogInformation("{Domain} Deleting documents matching predicate\n", domain);
        await _collection.DeleteManyAsync(predicate);
    }
}