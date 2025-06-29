using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TrafficDataAnalyticsData.Collections;

public class DailySummary
{
    [BsonId]
    public string SummaryId { get; set; }

    [BsonElement("intersection_id")]
    public string IntersectionId { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("avg_wait_time")]
    public float AverageWaitTime { get; set; }

    [BsonElement("peak_hours")]
    public BsonDocument PeakHours { get; set; }

    [BsonElement("total_vehicle_count")]
    public int TotalVehicleCount { get; set; }
}
