namespace TrafficLightControllerStore.Models.Dtos;

public class ControlEventDto
{
    public string Intersection { get; set; } = string.Empty;  // e.g., "ekklhsia"
    public string Light { get; set; } = string.Empty;         // e.g., "west"
    public string Command { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
