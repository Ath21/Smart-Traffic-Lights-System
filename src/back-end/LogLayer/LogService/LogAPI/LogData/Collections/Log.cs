/*
 * LogData.Collections.Log
 *
 * Log is a class that represents a log entry in the MongoDB database.
 * It contains properties for the log ID, log level, service name, message, and timestamp.
 * The class is decorated with attributes from the MongoDB.Bson.Serialization.Attributes namespace
 * to specify how the properties should be serialized to and from BSON format.
 * This class is used by the LogDbContext to perform CRUD operations on log entries in the MongoDB database.
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

public class Log
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("log_level")]
    public string LogLevel { get; set; } = "info";
    [BsonElement("service")]
    public string Service { get; set; }
    [BsonElement("message")]
    public string Message { get; set; }
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
