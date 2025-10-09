namespace SensorStore.Publishers.Count;

public interface ISensorCountPublisher
{
    Task PublishVehicleCountAsync(
        int count,
        double avgSpeed,
        double avgWait,
        double flowRate,
        Dictionary<string, int>? breakdown = null,
        Guid? correlationId = null);
    Task PublishPedestrianCountAsync(
        int count,
        Guid? correlationId = null);
    Task PublishCyclistCountAsync(
        int count,
        double avgSpeed = 0,
        double flowRate = 0,
        Dictionary<string, int>? breakdown = null,
        Guid? correlationId = null);
}
