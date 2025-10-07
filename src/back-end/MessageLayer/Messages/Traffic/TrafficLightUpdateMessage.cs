// traffic.light.update.{intersection}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
//
// Published by : Traffic Light Coordinator Service
// Consumed by  : Intersection Controller Service
public class TrafficLightUpdateMessage : BaseMessage
{
    public int LightId { get; set; }
    public TrafficLightState CurrentState { get; set; }
    public bool IsOperational { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
