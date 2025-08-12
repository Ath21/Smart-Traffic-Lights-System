using System;

namespace TrafficLightControlStore.Publishers.Light;

public interface ITrafficLightUpdatePublisher
{
    Task PublishUpdateAsync(string intersectionId, string state);
}
