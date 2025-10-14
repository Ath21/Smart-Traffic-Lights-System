using System;
using Messages.Traffic;

namespace IntersectionControllerStore.Publishers.Priority;

public interface IPriorityPublisher
{
    Task PublishPriorityCountAsync(PriorityCountMessage msg);
    Task PublishPriorityEventAsync(PriorityEventMessage msg);
}
