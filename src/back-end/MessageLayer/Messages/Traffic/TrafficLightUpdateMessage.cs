// traffic.light.update.{intersection}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
//
// Published by : Traffic Light Coordinator Service
// Consumed by  : Intersection Controller Service
public class TrafficLightUpdateMessage : BaseMessage
{
    // Light configuration template (applies to all lights in intersection)
    public bool IsOperational { get; set; } = true;

    // Operational mode
    public string CurrentMode { get; set; } = "Standard"; // Standard, Peak, Night, Manual, Failover, Emergency
    public string TimePlan { get; set; } = "Day";         // Day, Night, RushHour, Holiday

    // Timing and synchronization
    public Dictionary<string, int> PhaseDurations { get; set; } = new(); // { "Green": 30, "Yellow": 5, "Red": 25 }
    public int CycleDurationSec { get; set; } = 60;       // Total duration of one full cycle (sum of phases)
    public int GlobalOffsetSec { get; set; } = 0;         // Intersection-level offset (for city-wide sync)

    // Optional per-light offsets for fine-tuning (assigned by Coordinator or Intersection Controller)
    public Dictionary<int, int> LightOffsets { get; set; } = new(); // { 101: 0, 102: 5, 103: 10 }

    // Traceability
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}

/*

traffic.light.update.agioy-spyridonos

{
  "CorrelationId": "c98e1c40-4e5b-4a94-a0e4-bbbd7333f8ff",
  "Timestamp": "2025-10-08T07:30:00Z",
  "SourceServices": ["Traffic Light Coordinator Service"],
  "DestinationServices": ["Intersection Controller Service"],

  "IntersectionId": 2,
  "IntersectionName": "Agiou Spyridonos",

  "IsOperational": true,
  "CurrentMode": "Standard",
  "TimePlan": "MorningRush",

  "PhaseDurations": { "Green": 40, "Yellow": 5, "Red": 15 },
  "CycleDurationSec": 60,
  "GlobalOffsetSec": 15,
  "LightOffsets": { "101": 0, "102": 5, "103": 10 },

  "LastUpdate": "2025-10-08T07:30:00Z"
}


traffic.light.update.anatoliki-pyli

{
  "CorrelationId": "dc98f94c-5c46-46b9-a67d-6f7d5f8089f9",
  "Timestamp": "2025-10-07T23:59:00Z",
  "SourceServices": ["Traffic Light Coordinator Service"],
  "DestinationServices": ["Intersection Controller Service"],
  
  "IntersectionId": 4,
  "IntersectionName": "Anatoliki Pyli",
  "LightId": 201,
  "LightName": "anatoliki-pyli201",

  "IsOperational": true,
  "CurrentMode": "Night",
  "TimePlan": "Night",
  "PhaseDurations": { "Green": 15, "Yellow": 5, "Red": 30 },
  "LastUpdate": "2025-10-07T23:59:00Z"
}

traffic.light.update.kentriki-pyli

{
  "CorrelationId": "ea61dbf7-b97a-4c5e-8c4e-9ac9cfef2a77",
  "Timestamp": "2025-10-08T00:00:00Z",
  "SourceServices": ["Traffic Light Coordinator Service"],
  "DestinationServices": ["Intersection Controller Service"],

  "IntersectionId": 5,
  "IntersectionName": "Kentriki Pyli",

  "IsOperational": true,
  "CurrentMode": "Night",
  "TimePlan": "Night",

  "PhaseDurations": { "Green": 15, "Yellow": 5, "Red": 30 },
  "CycleDurationSec": 50,
  "GlobalOffsetSec": 0,
  "LightOffsets": { "501": 0, "502": 0 },

  "LastUpdate": "2025-10-08T00:00:00Z"
}


*/