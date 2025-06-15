using System;
using LogData.Collections;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LogData;

public class LogDbContext
{
    private readonly IMongoCollection<Log> _logsCollection; 

    public LogDbContext(IOptions<LogDbSettings> userLogsDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            userLogsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            userLogsDatabaseSettings.Value.DatabaseName);

        _logsCollection = mongoDatabase.GetCollection<Log>(
            userLogsDatabaseSettings.Value.LogsCollectionName);
    }

    public IMongoCollection<Log> LogsCollection => _logsCollection;
}
