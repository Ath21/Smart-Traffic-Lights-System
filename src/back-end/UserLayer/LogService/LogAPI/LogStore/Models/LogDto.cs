using System;

namespace LogStore.Models;

public class LogDto
{
    public string LogLevel { get; set; } = "info";
    public string Service { get; set; } = "";
    public string Message { get; set; } = "";
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
