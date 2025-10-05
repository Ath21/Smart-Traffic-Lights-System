using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

// Updated by : Log Service
// Read by    : Log Service
[BsonDiscriminator("audit_logs")]
[BsonIgnoreExtraElements]
public class AuditLogCollection
{
    // Unique identifier for the audit log entry
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string AuditId { get; set; } = string.Empty;

    // Timestamp of the logged event (UTC)
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Layer of origin (Traffic, Sensor, User, etc.)
    [BsonElement("layer")]
    public string Layer { get; set; } = string.Empty;

    // Service name that produced the log (e.g. "DetectionService")
    [BsonElement("service")]
    public string Service { get; set; } = string.Empty;

    // Action or operation performed (e.g. "UserLogin", "ConfigUpdate")
    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    // Human-readable message describing the event
    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    // Optional metadata for extra context (requestId, IP, payload)
    [BsonElement("metadata")]
    public BsonDocument Metadata { get; set; } = new();
}
