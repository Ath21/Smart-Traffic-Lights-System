using System;

namespace LogStore.Models.Requests;

public class SearchLogsRequest
{
    public string? ServiceName { get; set; }
    public string? ErrorType { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
