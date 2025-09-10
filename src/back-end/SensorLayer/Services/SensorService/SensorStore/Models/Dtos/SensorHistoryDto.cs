namespace SensorStore.Models.Dtos;


public class SensorHistoryDto
{
    public DateTime Timestamp { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
}