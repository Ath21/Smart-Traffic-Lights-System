namespace DetectionStore.Publishers.Event;

public interface IDetectionEventPublisher
{
    Task PublishEmergencyVehicleAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
    Task PublishPublicTransportAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
    Task PublishIncidentAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
}
