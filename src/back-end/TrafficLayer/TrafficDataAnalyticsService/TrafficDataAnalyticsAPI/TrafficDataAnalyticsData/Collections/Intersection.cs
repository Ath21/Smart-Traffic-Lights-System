using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TrafficDataAnalyticsData.Collections;

public class Intersection
{
    [BsonId]
    public string IntersectionId { get; set; }

    [BsonElement("location_name")]
    public string LocationName { get; set; }

    [BsonElement("coordinates")]
    public BsonDocument Coordinates { get; set; }

    [BsonElement("status")]
    public string Status { get; set; }
}
