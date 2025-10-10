using System;

namespace LogStore.Models.Responses;

public class SearchLogResponse
{
    public string LogType { get; set; } = string.Empty;  // "Audit" | "Error" | "Failover"

    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }

    public string Layer { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;

    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; } = string.Empty;

    public List<int>? LightId { get; set; }
    public List<string>? TrafficLight { get; set; }

    // Common fields
    public string? Action { get; set; }
    public string? Message { get; set; }

    // Type-specific fields
    public string? ErrorType { get; set; }  // For Error logs
    public string? Context { get; set; }    // For Failover logs
    public string? Reason { get; set; }     // For Failover logs
    public string? Mode { get; set; }       // For Failover logs

    public Dictionary<string, string>? Metadata { get; set; }
}
