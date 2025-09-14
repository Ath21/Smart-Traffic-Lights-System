namespace TrafficLightControllerStore.Models.Dtos;

public class TrafficLightDto
{
    public string Intersection { get; set; } = string.Empty;
    public string Light { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    // Manual override info
    public int? Duration { get; set; }
    public string? OverrideReason { get; set; }
    public DateTime? OverrideExpiresAt { get; set; }

    // Metadata
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
