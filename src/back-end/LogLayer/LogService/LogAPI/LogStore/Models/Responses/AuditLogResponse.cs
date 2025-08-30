using System;

namespace LogStore.Models.Responses;

public class AuditLogResponse
{
    public Guid LogId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}