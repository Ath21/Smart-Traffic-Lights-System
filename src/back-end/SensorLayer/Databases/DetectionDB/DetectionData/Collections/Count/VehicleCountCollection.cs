using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Count;

// Updated by : Sensor Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("vehicle_count")]
[BsonIgnoreExtraElements]
public class VehicleCountCollection
{
    // unique record ID
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string VehicleId { get; set; } = string.Empty; 

     // reference to intersection
    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    // readable intersection name
    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty;

    // capture time (UTC)
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // total number of vehicles detected
    [BsonElement("count_total")]
    public int CountTotal { get; set; } = 0;

    // mean vehicle speed
    [BsonElement("average_speed_kmh")]
    public double AverageSpeedKmh { get; set; } = 0.0;

    // mean waiting time at intersection
    [BsonElement("average_wait_time_sec")]
    public double AverageWaitTimeSec { get; set; } = 0.0;

    // distribution by traffic direction
    [BsonElement("count_by_direction")]
    public Dictionary<string, int> CountByDirection { get; set; } = new()
    {
        { "north", 0 },
        { "south", 0 },
        { "east", 0 },
        { "west", 0 }
    }; 

    // category-based breakdown (vehicle types)
    [BsonElement("vehicle_breakdown")]
    public Dictionary<string, int> VehicleBreakdown { get; set; } = new()
    {
        { "car", 0 },
        { "truck", 0 },
        { "motorcycle", 0 },
        { "public_transport", 0 },
        { "emergency", 0 }
    };
}