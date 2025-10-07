public abstract class BaseMessage
{
    // Traceability properties
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Message-2-message communication
    public List<string> SourceServices { get; set; } = new();
    public List<string> DestinationServices { get; set; } = new();

    // Intersection properties
    public int IntersectionId { get; set; } = 0;
    public string IntersectionName { get; set; } = string.Empty;

    // Optional metadata for additional context
    public Dictionary<string, string>? Metadata { get; set; } = new();
}