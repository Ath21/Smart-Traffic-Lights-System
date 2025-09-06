using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid LogId { get; set; }

    [BsonElement("service_name")]
    public string ServiceName { get; set; } = null!;

    [BsonElement("action")]
    public string Action { get; set; } = null!;

    [BsonElement("message")]
    public string Message { get; set; } = null!;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("metadata")]
    public BsonDocument Metadata { get; set; } = new();
}