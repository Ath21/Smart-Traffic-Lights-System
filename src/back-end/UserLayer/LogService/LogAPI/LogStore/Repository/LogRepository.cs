using LogData.Collections;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using LogData;

namespace LogStore.Repository;

public class LogRepository : ILogRepository
{
    private readonly IMongoCollection<Log> _logsCollection;

    public LogRepository(IOptions<LogDbSettings> userLogsDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            userLogsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            userLogsDatabaseSettings.Value.DatabaseName);

        _logsCollection = mongoDatabase.GetCollection<Log>(
            userLogsDatabaseSettings.Value.LogsCollectionName);
    }

    public async Task CreateAsync(Log newLog)
    {
        await _logsCollection.InsertOneAsync(newLog);
    }

    public async Task<List<Log>> GetAllAsync()
    {
        return await _logsCollection.Find(_ => true).ToListAsync();
    }

    public async Task<List<Log?>> GetAsync(string Id)
    {
        return await _logsCollection.Find(x => x.Id == Id).ToListAsync();
    }
}
