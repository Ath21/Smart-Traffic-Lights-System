namespace SensorStore.Publishers.Count;

public interface ISensorCountPublisher
{
    Task PublishVehicleCountAsync(
        int count,
        double avgSpeed,
        double avgWait,
        double flowRate,
        Dictionary<string, int>? breakdown = null,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
    Task PublishPedestrianCountAsync(
        int count,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
    Task PublishCyclistCountAsync(
        int count,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
}
