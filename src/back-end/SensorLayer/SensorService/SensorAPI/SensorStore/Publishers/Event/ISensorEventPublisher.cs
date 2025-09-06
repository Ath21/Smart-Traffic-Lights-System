using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers.Event;

public interface ISensorEventPublisher
{
    Task PublishVehicleCountAsync(Guid intersectionId, int count, float avgSpeed);
    Task PublishPedestrianCountAsync(Guid intersectionId, int count);
    Task PublishCyclistCountAsync(Guid intersectionId, int count);
}
