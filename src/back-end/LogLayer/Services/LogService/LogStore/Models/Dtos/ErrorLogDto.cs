using System;

namespace LogStore.Models.Dtos;

public class ErrorLogDto
{
    public Guid LogId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ErrorType { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
