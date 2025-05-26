using System;
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
    [BsonElement("trace_id")]
    public string? TraceId { get; set; }
}
