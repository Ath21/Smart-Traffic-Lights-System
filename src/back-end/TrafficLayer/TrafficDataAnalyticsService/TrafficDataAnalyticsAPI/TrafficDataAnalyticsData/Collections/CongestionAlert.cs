using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TrafficDataAnalyticsData.Collections;

public class CongestionAlert
{
    [BsonId]
    public string AlertId { get; set; }

    [BsonElement("intersection_id")]
    public string IntersectionId { get; set; }

    [BsonElement("severity")]
    public string Severity { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
}
