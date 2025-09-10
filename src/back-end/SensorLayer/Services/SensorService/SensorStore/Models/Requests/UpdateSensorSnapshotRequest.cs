namespace SensorStore.Models.Requests;

public class UpdateSensorSnapshotRequest
{
    public Guid IntersectionId { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
    public float AvgSpeed { get; set; }   // optional metric for vehicle flow
}