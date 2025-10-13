using Messages.Sensor;

namespace DetectionStore.Publishers.Event;

public interface IDetectionEventPublisher
{
    Task<DetectionEventMessage> PublishEmergencyVehicleAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
    Task<DetectionEventMessage> PublishPublicTransportAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
    Task<DetectionEventMessage> PublishIncidentAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
}
