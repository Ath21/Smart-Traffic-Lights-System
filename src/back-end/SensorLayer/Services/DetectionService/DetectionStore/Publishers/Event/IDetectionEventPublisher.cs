namespace DetectionStore.Publishers.Event;

public interface IDetectionEventPublisher
{
    Task PublishEmergencyVehicleAsync(
        string vehicleType,
        int speed_kmh,
        string direction,
        Guid? correlationId = null);
    Task PublishPublicTransportAsync(
        string mode,
        string line,
        int arrival_estimated_sec,
        string direction,
        Guid? correlationId = null);
    Task PublishIncidentAsync(
        string type,
        int severity,
        string description,
        string direction,
        Guid? correlationId = null);
}
