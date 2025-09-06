using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers;

public interface ISensorEventPublisher
{
    Task PublishVehicleCountAsync(Guid intersectionId, int count, DateTime detectedAt);
    Task PublishPedestrianCountAsync(Guid intersectionId, int count, DateTime detectedAt);
    Task PublishCyclistCountAsync(Guid intersectionId, int count, DateTime detectedAt);
}
