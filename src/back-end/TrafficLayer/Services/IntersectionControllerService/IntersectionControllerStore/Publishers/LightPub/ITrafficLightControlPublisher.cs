using System;

namespace IntersectionControllerStore.Publishers.LightPub;

public interface ITrafficLightControlPublisher
{
    Task PublishControlAsync(string intersection, string light, string newState);
}
