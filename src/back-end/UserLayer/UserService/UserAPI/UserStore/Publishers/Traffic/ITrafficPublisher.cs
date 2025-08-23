using System;

namespace UserStore.Publishers.Traffic;

public interface ITrafficPublisher
{
    Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState);
    Task PublishUpdateAsync(Guid intersectionId, Guid lightId, string currentState);
}
