using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TrafficDataAnalyticsData.Collections;

public class VehicleCount
{
    [BsonId]
    public string Id { get; set; }

    [BsonElement("intersection_id")]
    public string IntersectionId { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("count")]
    public int Count { get; set; }

    [BsonElement("lane_id")]
    public string LaneId { get; set; }
}
