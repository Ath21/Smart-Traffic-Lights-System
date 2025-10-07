// priority.detection.{intersection}.{event}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
// {event}        : emergency, incident, public-transport
//
// Published by : Intersection Controller Service
// Consumed by  : Traffic Light Coordinator Service
public class PriorityEventMessage : BaseMessage
{
    public string EventType { get; set; }
    public string? VehicleType { get; set; }
    public int PriorityLevel { get; set; }  // 1 (Low), 2 (Medium), 3 (High)
    public string Direction { get; set; } 
}
