namespace TrafficLightData.Entities;

public class TrafficLight
{
    public int LightId { get; set; }               // PK
    public int IntersectionId { get; set; }       // FK

    public TrafficLightState CurrentState { get; set; } = TrafficLightState.RED;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Intersection? Intersection { get; set; }
}
