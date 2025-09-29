using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers.Count;

public interface ISensorCountPublisher
{
    Task PublishVehicleCountAsync(int count, float avgSpeed = 0);
    Task PublishPedestrianCountAsync(int count);
    Task PublishCyclistCountAsync(int count);
}
