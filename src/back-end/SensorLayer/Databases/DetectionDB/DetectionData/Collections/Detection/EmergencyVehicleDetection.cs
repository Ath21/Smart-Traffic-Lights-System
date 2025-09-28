using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class EmergencyVehicleDetection
{
    [BsonId]
    public ObjectId Id { get; set; }

    public DateTime Timestamp { get; set; }
    public int IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string Type { get; set; } // ambulance, firetruck
    public int PriorityLevel { get; set; }
}
