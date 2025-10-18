namespace Messages.Log;

public class LogMessage
{
    public string Source { get; set; } = null!;
    public string Type { get; set; } = "audit";
    public string Message { get; set; } = null!;
    public string? Category { get; set; }
    public string Level { get; set; } = "info";
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Data { get; set; }
}
