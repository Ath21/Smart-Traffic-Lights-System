using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class PublicTransportDetection
{
    [BsonId]
    public ObjectId Id { get; set; }

    public DateTime Timestamp { get; set; }
    public int IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string Mode { get; set; } // bus, tram
    public string RouteId { get; set; }
}
