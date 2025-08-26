using System;

namespace TrafficDataAnalyticsStore.Models.Requests;

public class GetSummaryRequest
{
    public Guid IntersectionId { get; set; }
    public DateTime Date { get; set; }
}