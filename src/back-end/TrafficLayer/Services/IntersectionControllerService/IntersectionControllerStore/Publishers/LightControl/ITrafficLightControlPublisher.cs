using System;
using Messages.Traffic;

namespace IntersectionControllerStore.Publishers.LightControl;

public interface ITrafficLightControlPublisher
{
    Task PublishControlAsync(TrafficLightControlMessage msg);
}
