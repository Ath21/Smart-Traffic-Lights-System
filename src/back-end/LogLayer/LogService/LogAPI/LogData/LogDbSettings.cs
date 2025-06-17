/*
 * LogData.LogDbSettings
 *
 * LogDbSettings is a class that holds the settings for connecting to the MongoDB database.
 * It includes properties for the connection string, database name, and logs collection name.
 * These settings are typically configured in the application settings file and injected into the application
 * using dependency injection.
 * This class is used by the LogDbContext to establish a connection to the MongoDB database
 * and access the logs collection.
 */
namespace LogData;

public class LogDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string LogsCollectionName { get; set; } = null!;   
}

