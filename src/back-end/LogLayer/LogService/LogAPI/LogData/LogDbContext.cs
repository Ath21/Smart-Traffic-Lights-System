using LogData.Collections;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LogData;

public class LogDbContext
{
    private readonly IMongoDatabase _database;

    public LogDbContext(IOptions<LogDbSettings> logDbSettings)
    {
        var mongoClient = new MongoClient(logDbSettings.Value.ConnectionString);
        _database = mongoClient.GetDatabase(logDbSettings.Value.DatabaseName);

        AuditLogs = _database.GetCollection<AuditLog>(logDbSettings.Value.AuditLogsCollectionName);
        ErrorLogs = _database.GetCollection<ErrorLog>(logDbSettings.Value.ErrorLogsCollectionName);
    }

    public IMongoCollection<AuditLog> AuditLogs { get; }
    public IMongoCollection<ErrorLog> ErrorLogs { get; }
}