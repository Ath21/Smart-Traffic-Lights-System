using System;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class PublicTransportDetection
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
    [BsonElement("intersectionId")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid IntersectionId { get; set; }
    [BsonElement("detected")]
    public bool Detected { get; set; }
    [BsonElement("mode")]
    public string Mode { get; set; } = string.Empty;
    [BsonElement("routeId")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string? RouteId { get; set; }
}
