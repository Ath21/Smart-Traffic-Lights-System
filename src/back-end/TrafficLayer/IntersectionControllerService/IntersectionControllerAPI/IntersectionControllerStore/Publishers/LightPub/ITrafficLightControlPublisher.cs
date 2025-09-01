using System;

namespace IntersectionControlStore.Publishers.LightPub;

public interface ITrafficLightControlPublisher
{
    Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState);
}
