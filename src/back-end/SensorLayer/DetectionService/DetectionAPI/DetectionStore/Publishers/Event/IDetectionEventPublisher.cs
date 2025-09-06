using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers;

public interface IDetectionEventPublisher
{
    Task PublishEmergencyVehicleAsync(Guid intersectionId, DateTime detectedAt);
    Task PublishPublicTransportAsync(Guid intersectionId, DateTime detectedAt, string lineId);
    Task PublishIncidentAsync(Guid intersectionId, string description, DateTime detectedAt);
}
