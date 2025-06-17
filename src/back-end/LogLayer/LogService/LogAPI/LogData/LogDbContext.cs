/*
 *  LogData.LogDbContext
 *
 *  LogDbContext is a class that provides access to the MongoDB database
 *  for storing and retrieving log data. It uses the MongoDB.Driver package
 *  to connect to the database and perform operations on the logs collection.
 */
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
