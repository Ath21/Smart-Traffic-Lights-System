using System;

namespace UserData.Entities;

public class AuditLog
{
    public Guid LogId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}
