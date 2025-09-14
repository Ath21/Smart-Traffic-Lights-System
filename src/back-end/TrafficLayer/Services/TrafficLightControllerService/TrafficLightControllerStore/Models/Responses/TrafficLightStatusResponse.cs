namespace TrafficLightControllerStore.Models.Responses;

public class TrafficLightStatusResponse
{
    public string Intersection { get; set; } = string.Empty;
    public string Light { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;

    public int? Duration { get; set; }
    public string? OverrideReason { get; set; }
    public DateTime? OverrideExpiresAt { get; set; }
}
