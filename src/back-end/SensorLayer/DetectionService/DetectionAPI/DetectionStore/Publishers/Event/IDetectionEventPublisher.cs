using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers.Event;

public interface IDetectionEventPublisher
{
    Task PublishEmergencyVehicleAsync(Guid intersectionId, bool detected);
    Task PublishPublicTransportAsync(Guid intersectionId, string? routeId);
    Task PublishIncidentAsync(Guid intersectionId, string description);
}
