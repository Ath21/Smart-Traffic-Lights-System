using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

public class FailoverLog
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid LogId { get; set; }

    [BsonElement("service_name")]
    public string ServiceName { get; set; } = null!;

    [BsonElement("context")]
    public string Context { get; set; } = null!; // e.g. ekklhsia.west, detection.cache

    [BsonElement("reason")]
    public string Reason { get; set; } = null!; // e.g. RedisUnavailable

    [BsonElement("mode")]
    public string Mode { get; set; } = null!; // e.g. BlinkingYellow, SafeStop

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("metadata")]
    public BsonDocument Metadata { get; set; } = new();
}
