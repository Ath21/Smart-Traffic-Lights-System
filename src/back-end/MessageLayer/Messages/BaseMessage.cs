public abstract class BaseMessage
{
    // Traceability properties
    public Guid CorrelationId { get; set; } 
    public DateTime Timestamp { get; set; } 

    // Message-2-message communication
    public List<string>? SourceServices { get; set; } 
    public List<string>? DestinationServices { get; set; } 

    // Intersection properties
    public int IntersectionId { get; set; }
    public string? IntersectionName { get; set; }

    // Optional metadata for additional context
    public Dictionary<string, string>? Metadata { get; set; }
}