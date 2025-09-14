namespace TrafficLightControllerStore.Models.Responses;

public class ControlEventResponse
{
    public string Intersection { get; set; } = string.Empty;
    public string Light { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
