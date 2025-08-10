using System;
using TrafficMessages.Light;

namespace IntersectionControlStore.Publishers.LightPub;

public interface ITrafficLightControlPublisher
{
    public Task PublishTrafficLightControlAsync(TrafficLightControl controlMessage);
}
