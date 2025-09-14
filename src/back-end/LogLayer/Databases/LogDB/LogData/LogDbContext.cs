using LogData.Collections;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LogData;

public class LogDbContext
{
    private readonly IMongoDatabase _database;

    public LogDbContext(IOptions<LogDbSettings> logDbSettings)
    {
        var mongoClient = new MongoClient(logDbSettings.Value.ConnectionString);
        _database = mongoClient.GetDatabase(logDbSettings.Value.Database);

        AuditLogs = _database.GetCollection<AuditLog>(logDbSettings.Value.AuditLogsCollection);
        ErrorLogs = _database.GetCollection<ErrorLog>(logDbSettings.Value.ErrorLogsCollection);
        FailoverLogs = _database.GetCollection<FailoverLog>(logDbSettings.Value.FailoverLogsCollection);
    }

    public IMongoCollection<AuditLog> AuditLogs { get; }
    public IMongoCollection<ErrorLog> ErrorLogs { get; }
    public IMongoCollection<FailoverLog> FailoverLogs { get; }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            var command = new BsonDocument("ping", 1);
            await _database.RunCommandAsync<BsonDocument>(command);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
