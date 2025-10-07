// priority.count.{intersection}.{type}
// 
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
// {type}         : vehicle, pedestrian, cyclist
//
// Published by : Intersection Controller Service
// Consumed by  : Traffic Light Coordinator Service
public class PriorityCountMessage : BaseMessage
{
    public string CountType { get; set; }  // Vehicle, Pedestrian, Cyclist
    public int TotalCount { get; set; }     
    public int PriorityLevel { get; set; }   // 1 (Low), 2 (Medium), 3 (High)
    public bool IsThresholdExceeded { get; set; } // True if congestion threshold exceeded
}