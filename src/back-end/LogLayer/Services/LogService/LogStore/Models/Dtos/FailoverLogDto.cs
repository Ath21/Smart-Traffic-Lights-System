namespace LogStore.Models.Dtos;

public class FailoverLogDto
{
    public Guid LogId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string Context { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string Mode { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
