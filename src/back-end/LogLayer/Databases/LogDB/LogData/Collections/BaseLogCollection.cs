using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

public abstract class BaseLogCollection
{
    [BsonElement("correlation_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid CorrelationId { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("layer")]
    public string? Layer { get; set; }

    [BsonElement("service")]
    public string? Service { get; set; }

    [BsonElement("action")]
    public string? Action { get; set; }

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }
}
