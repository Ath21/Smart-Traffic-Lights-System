using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class IncidentDetection
{
    [BsonId]
    public ObjectId Id { get; set; }

    public DateTime Timestamp { get; set; }
    public int IntersectionId { get; set; }
    public string Type { get; set; } // collision
    public int Severity { get; set; } // 1-5
    public string Description { get; set; }
}