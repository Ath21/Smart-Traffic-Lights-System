namespace TrafficMessages;

// traffic.analytics.congestion.<intersection_id>
    public record TrafficCongestionMessage(
        Guid AlertId,
        Guid IntersectionId,
        string CongestionLevel,
        string Message,
        DateTime Timestamp
    );
 
