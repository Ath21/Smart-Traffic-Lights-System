using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

// Updated by : Detection Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("incident_detections")]
[BsonIgnoreExtraElements]
public class IncidentDetectionCollection
{
    // unique incident ID
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string IncidentId { get; set; } = string.Empty;

    // reference to intersection
    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    // readable intersection name
    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty;

     // report time (UTC)
    [BsonElement("reportedAt")]
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    // description of the incident
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    // severity level (1 = low, 5 = high)
    [BsonElement("severity")]
    public int Severity { get; set; } = 1;
}
