using System;

namespace TrafficLightControlStore.Publishers;

public interface ITrafficLightUpdatePublisher
{
    Task PublishUpdateAsync(string intersectionId, string state);
}
