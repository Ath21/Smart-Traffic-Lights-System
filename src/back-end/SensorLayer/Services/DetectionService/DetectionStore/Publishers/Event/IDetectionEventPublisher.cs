using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers.Event;

public interface IDetectionEventPublisher
{
    Task PublishEmergencyVehicleAsync(string type, int priority, string direction);
    Task PublishPublicTransportAsync(string mode, string direction);
    Task PublishIncidentAsync(string type, int severity, string description, string direction);
}
