using System;

namespace IntersectionControllerStore.Publishers.LightPub;

public interface ITrafficLightControlPublisher
{
    Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState);
}
