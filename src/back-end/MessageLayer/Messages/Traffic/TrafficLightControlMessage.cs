// traffic.light.control.{intersection}.{light}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
// {light}        : agiou-spyridonos101, dimitsanas102, 
//                  anatoliki-pyli201, agiou-spyridonos202, 
//                  dytiki-pyli301, dimitsanas-north302, dimitsanas-south303, 
//                  dimitsanas401, edessis402, korytsas403, 
//                  kentriki-pyli501, agiou-spyridonos502
//
// Published by : Intersection Controller Service, User Service
// Consumed by  : Traffic Light Controller Service
public class TrafficLightControlMessage : BaseMessage
{
    public int LightId { get; set; }
    public string LightName { get; set; }
    public string OperationalMode { get; set; } // Normal, Flashing, Off
    public Dictionary<string, int> PhaseDurations { get; set; } // e.g., {"Green": 30, "Yellow": 5, "Red": 25}
}