using System;

namespace TrafficAnalyticsStore.Models.Requests;

public class GetIncidentsRequest
{
    public Guid IntersectionId { get; set; }
}