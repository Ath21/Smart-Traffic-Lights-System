using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers.Count;

public interface ISensorCountPublisher
{
    Task PublishVehicleCountAsync(int count, double avgSpeed, string direction);
    Task PublishPedestrianCountAsync(int count, string direction);
    Task PublishCyclistCountAsync(int count, string direction);
}
