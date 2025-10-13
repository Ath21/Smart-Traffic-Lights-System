namespace Messages;

public abstract class BaseMessage
{
    // Traceability properties
    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }

    // Layer-2-layer communication
    public string? SourceLayer { get; set; }
    public List<string>? DestinationLayer { get; set; }

    // Service-2-Service communication
    public string? SourceService { get; set; }
    public List<string>? DestinationServices { get; set; }

    // Optional metadata for additional context
    public Dictionary<string, string>? Metadata { get; set; }
}