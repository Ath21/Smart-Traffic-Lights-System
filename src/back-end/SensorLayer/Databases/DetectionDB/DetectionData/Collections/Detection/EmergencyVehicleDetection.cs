using System;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class EmergencyVehicleDetection
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
    [BsonElement("type")]
    public string? Type { get; set; }
    [BsonElement("priorityLevel")]
    public int? PriorityLevel { get; set; }
}
