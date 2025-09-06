using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers.Count;

public interface ISensorCountPublisher
{
    Task PublishVehicleCountAsync(Guid intersectionId, int count, float avgSpeed);
    Task PublishPedestrianCountAsync(Guid intersectionId, int count);
    Task PublishCyclistCountAsync(Guid intersectionId, int count);
}
