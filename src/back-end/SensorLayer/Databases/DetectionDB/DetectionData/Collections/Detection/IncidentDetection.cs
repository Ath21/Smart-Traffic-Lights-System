using System;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class IncidentDetection
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
    [BsonElement("intersectionId")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid IntersectionId { get; set; }
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;
    [BsonElement("severity")]   
    public int Severity { get; set; }
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
}