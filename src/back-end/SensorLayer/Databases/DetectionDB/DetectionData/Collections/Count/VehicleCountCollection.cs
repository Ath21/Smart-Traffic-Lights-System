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
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string VehicleId { get; set; } = string.Empty;

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; } = 0;

    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("count_total")]
    public int CountTotal { get; set; } = 0;

    [BsonElement("average_speed_kmh")]
    public double AverageSpeedKmh { get; set; } = 0.0;

    [BsonElement("average_wait_time_sec")]
    public double AverageWaitTimeSec { get; set; } = 0.0;

    [BsonElement("count_by_direction")]
    public Dictionary<string, int> CountByDirection { get; set; } = new();

    [BsonElement("vehicle_breakdown")]
    public Dictionary<string, int> VehicleBreakdown { get; set; } = new(); // e.g., car, truck, bus, motorcycle
}

/*

{
  "intersection_id": 2,
  "intersection": "Agiou Spyridonos",
  "timestamp": "2025-10-07T13:50:21Z",
  "count_total": 48,
  "average_speed_kmh": 39.4,
  "average_wait_time_sec": 14.2,
  "count_by_direction": {
    "north": 0,
    "south": 34,
    "east": 14,
    "west": 0
  },
  "vehicle_breakdown": {
    "car": 38,
    "bus": 4,
    "truck": 2,
    "motorcycle": 4
  }
}

*/


