using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers.Count;

public interface ISensorCountPublisher
{
    Task PublishVehicleCountAsync(int intersectionId, int count, float avgSpeed);
    Task PublishPedestrianCountAsync(int intersectionId, int count);
    Task PublishCyclistCountAsync(int intersectionId, int count);
}
