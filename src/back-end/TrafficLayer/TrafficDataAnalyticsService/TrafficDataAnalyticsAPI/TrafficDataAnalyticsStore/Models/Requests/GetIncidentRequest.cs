using System;

namespace TrafficDataAnalyticsStore.Models.Requests;

public class GetIncidentsRequest
{
    public Guid IntersectionId { get; set; }
}