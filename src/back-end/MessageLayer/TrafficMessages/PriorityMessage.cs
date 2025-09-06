namespace TrafficMessages;

// traffic.priority.*.{intersection_id}
public record PriorityMessage(
    Guid IntersectionId,
    string PriorityType,    
    Guid? DetectionId,      
    string? Reason,
    DateTime Timestamp
);
