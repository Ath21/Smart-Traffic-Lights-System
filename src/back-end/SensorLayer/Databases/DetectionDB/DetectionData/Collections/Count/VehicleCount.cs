using System;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Count;

public class VehicleCount
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
    [BsonElement("intersectionId")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid IntersectionId { get; set; }
    [BsonElement("laneId")]
    public string LaneId { get; set; } = string.Empty;
    [BsonElement("count")]
    public int Count { get; set; }
}
