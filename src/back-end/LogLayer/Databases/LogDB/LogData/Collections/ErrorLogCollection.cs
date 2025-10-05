using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

// Updated by : Log Service
// Read by    : Log Service
[BsonDiscriminator("error_logs")]
[BsonIgnoreExtraElements]
public class ErrorLogCollection
{
    // Unique identifier for the error log entry
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ErrorId { get; set; } = string.Empty;

    // Timestamp when the error occurred (UTC)
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Layer of origin (Traffic, Sensor, User, etc.)
    [BsonElement("layer")]
    public string Layer { get; set; } = string.Empty;

    // Service that encountered the error (e.g. "IntersectionControllerService")
    [BsonElement("service")]
    public string Service { get; set; } = string.Empty;

    // Type of error (e.g. "DatabaseError", "NetworkError", "TimeoutError")
    [BsonElement("error_type")]
    public string ErrorType { get; set; } = string.Empty;

    // Descriptive message of the issue
    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    // Additional debug metadata (stack trace, payload)
    [BsonElement("metadata")]
    public BsonDocument Metadata { get; set; } = new();
}