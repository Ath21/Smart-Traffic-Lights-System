using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

// Updated by : Detection Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("emergency_vehicle_detections")]
[BsonIgnoreExtraElements]
public class EmergencyVehicleDetectionCollection
{
    // unique detection ID
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string EmergencyId { get; set; } = string.Empty; 

    // reference to intersection
    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    // readable intersection name
    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty;

    // detection time (UTC)
    [BsonElement("detectedAt")]
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow; 

    // approach direction (e.g., "Dimitsanas")
    [BsonElement("direction")]
    public string Direction { get; set; } = string.Empty; 

    // priority level (1 = high, 5 = low)
    [BsonElement("priorityLevel")]
    public int PriorityLevel { get; set; } = 1;
}