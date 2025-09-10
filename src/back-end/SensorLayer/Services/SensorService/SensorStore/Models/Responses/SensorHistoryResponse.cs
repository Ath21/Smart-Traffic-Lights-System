public class SensorHistoryResponse
{
    public DateTime Timestamp { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
}